import sys
import os
import keras
import numpy as np
from keras.models import model_from_json
from keras import backend
import tensorflow as tf



current_directory_path = os.path.dirname(os.path.abspath(__file__))
config = tf.ConfigProto()
config.gpu_options.allow_growth = True
session = tf.Session(config=config)

def load_data(file_name, training):

    os.chdir(current_directory_path)
    os.chdir("..")
    if training:
        os.chdir("TensorflowData\\TrainingData\\")
    else:
        os.chdir("TensorflowData\\EvaluationData\\")
    base_path = os.getcwd()

    data = []
    with open(base_path + "\\" + file_name, "r") as file:
        for line in file:
            data.append(line.split(","))

    cleanData = []
    for entry in data:
        temp = []
        for item in entry:
            temp.append(float(item))
        cleanData.append(temp)
    output = np.array(cleanData, dtype=np.float64)

    output = output / np.linalg.norm(output)

    return output


def load_labels(file_name):
    os.chdir(current_directory_path)
    os.chdir("..")
    os.chdir("TensorflowData\\")
    base_path = os.getcwd()

    data = []
    with open(base_path + "\\" + file_name, "r") as file:
        for line in file:
            data.append(float(line))
    output = np.array(data, dtype=np.float64)

    return output


def train_model():
    trainingData = load_data("317396487match1.csv")
    np.append (trainingData, load_data("317396487match2.csv"))
    np.append(trainingData, load_data("317396487match3.csv"))
    np.append(trainingData, load_data("317396487match4.csv"))
    np.append(trainingData, load_data("325392389match1.csv"))
    np.append(trainingData, load_data("325392389match2.csv"))
    np.append(trainingData, load_data("325392389match3.csv"))
    np.append(trainingData, load_data("325392389match4.csv"))
    np.append(trainingData, load_data("325392389match5.csv"))
    np.append(trainingData, load_data("325392389match6.csv"))
    np.append(trainingData, load_data("325392389match7.csv"))

    labels = load_labels("317396487match1_training.csv")
    np.append(labels, load_labels("317396487match2_training.csv"))
    np.append(labels, load_labels("317396487match3_training.csv"))
    np.append(labels, load_labels("317396487match4_training.csv"))
    np.append(labels, load_labels("325392389match1_training.csv"))
    np.append(labels, load_labels("325392389match2_training.csv"))
    np.append(labels, load_labels("325392389match3_training.csv"))
    np.append(labels, load_labels("325392389match4_training.csv"))
    np.append(labels, load_labels("325392389match5_training.csv"))
    np.append(labels, load_labels("325392389match6_training.csv"))
    np.append(labels, load_labels("325392389match7_training.csv"))

    validationData = load_data("317396487match5.csv")
    validationLabels = load_data("317396487match5_training.csv")

    highlights_data = load_data("highlights.csv")
    highlights_labels = np.random.uniform(0.5, 1.0, highlights_data.shape[0])
    np.append(trainingData, highlights_data)
    np.append(labels, highlights_labels)

    # Model architecture
    model = keras.Sequential([
    keras.layers.Dense(128, activation='relu', input_shape=(7,)),
    keras.layers.Dense(128, activation='relu'),
    keras.layers.Dense(1, activation='sigmoid')])

    # Compiling
    model.compile(optimizer=keras.optimizers.Adam(0.0005),
                  loss='mse',
                  metrics=["mae", "mse"])

    # Train model on data
    print("TRAINING")
    model.fit(trainingData, labels, epochs=300, batch_size=5, validation_data=(validationData, validationLabels), verbose=2)
    print("PREDICTING")
    result = model.predict(validationData, batch_size=1)
    print(result)
    print("EVALUATING")
    evalation = model.evaluate(validationData, validationLabels, steps=50)
    print("test_loss = " + str(evalation[0]) + ", test_accuracy = " + str(evalation[1]))

    return model


def load_model():
    json_file = open("model.json", "r")
    loaded_model_json = json_file.read()
    json_file.close()
    loaded_model = model_from_json(loaded_model_json)
    loaded_model.load_weights("model.h5")
    loaded_model.compile(optimizer=keras.optimizers.Adam(0.0005),
                  loss='mse',
                  metrics=["mae", "mse"])
    print("model loaded")
    return loaded_model

def save_model(model):
    model_json = model.to_json()
    with open("model.json", "w") as json_file:
        json_file.write(model_json)
    model.save_weights("model.h5")
    print("model saved")

def model_exists():
    os.chdir(current_directory_path)
    os.chdir("..")
    os.chdir("TensorflowData\\")
    return os.path.isfile("model.json")


def main(video_data_path, predicted_data_path):
    if not model_exists():
        model = train_model()
        save_model(model)
    else:
        model = load_model()

    data = load_data(video_data_path, False)
    highlight = model.predict(data)
    outputList = highlight.tolist()

    os.chdir(current_directory_path)
    os.chdir("..")
    os.chdir("TensorflowData\\Predictions\\")

    with open(predicted_data_path, "w") as output:
        output.write(('\n'.join(str(num[0]) for num in highlight)))

    backend.clear_session()
    session.close()


if __name__ == "__main__":
    main("317396487match1.csv", "317396487match1_prediction.csv")
    #main(sys.argv[1], sys.argv[2])