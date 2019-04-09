import sys
import os
import re
import tensorflow as tf
import tensorflow.keras as keras
import numpy as np

current_directory_path = os.path.dirname(os.path.abspath(__file__))

def load_data():
    os.chdir(current_directory_path)
    os.chdir("..")
    os.chdir("Analyzed Broadcasts\\")
    base_path = os.getcwd()
    dirs = os.listdir(os.getcwd())
    data = []

    for dir in dirs:
        if dir.isnumeric():
            os.chdir(dir)
            data.append([dir, os.listdir(os.getcwd())])
            os.chdir("..")

    # Seperate content out into highlight and matches.
    hightlights = []
    matches = []
    for broadcast in data:
        hightlights.append([broadcast[0]])
        matches.append([broadcast[0]])
        for match in broadcast[1]:
            if 'highlight' in match:
                hightlights[-1].append(match)
            else:
                matches[-1].append(match)

    # Further eperate highlights and matches by their id highlight1, highlight2 ect
    segregated_highlights = {}
    segregated_matches = {}

    for collection in hightlights:
        segregated_highlights[collection[0]] = {}

    for collection in hightlights:
        first = True
        for hightlight in collection:
            if first:
                first = False
                continue
            number = re.findall(r'\d+', hightlight)[0]
            if number in segregated_highlights[collection[0]].keys():
                segregated_highlights[collection[0]][number].append(hightlight)
            else:
                segregated_highlights[collection[0]][number] = []
                segregated_highlights[collection[0]][number].append(hightlight)




    for collection in matches:
        segregated_matches[collection[0]] = {}

    for collection in matches:
        first = True
        for hightlight in collection:
            if first:
                first = False
                continue
            number = re.findall(r'\d+', hightlight)[0]
            if number in segregated_matches[collection[0]].keys():
                segregated_matches[collection[0]][number].append(hightlight)
            else:
                segregated_matches[collection[0]][number] = []
                segregated_matches[collection[0]][number].append(hightlight)

    sum_total = 0
    sum_baron = 0
    sum_dragon = 0
    sum_inhibitor = 0
    sum_kills = 0
    sum_turrets = 0
    sum_ultimates = 0

    # load each segment's file information into

    for folder in segregated_highlights.keys():
        for metric_group_path in segregated_highlights[folder].keys():
            for file_path in segregated_highlights[folder][metric_group_path]:
                with open(base_path + "\\" + folder + "\\" + file_path) as file:
                    input_raw = file.readlines()
                    sum = 0
                    # I sum each highlight video as they generally cover a 10 second period.
                    for item in input_raw:
                        sum += 1
                    sum_total += sum
                    if "baron" in file_path:
                        sum_baron += sum
                    if "dragon" in file_path:
                        sum_dragon += sum
                    if "inhibitor" in file_path:
                        sum_inhibitor += sum
                    if "kills" in file_path:
                        sum_kills += sum
                    if "turrets" in file_path:
                        sum_turrets += sum
                    if "ultimates" in file_path:
                        sum_ultimates += sum


    metric_group_highlights = []
    for folder in segregated_highlights.keys():
        for metric_group_path in segregated_highlights[folder].keys():
            metric_group_highlights.append({})
            for file_path in segregated_highlights[folder][metric_group_path]:
                with open(base_path + "\\" + folder + "\\" + file_path) as file:
                    input_raw = file.readlines()
                    sum = 0
                    # I sum each highlight video as they generally cover a 10 second period.
                    for item in input_raw:
                        sum += 1
                    if "baron" in file_path:
                        metric_group_highlights[-1]["baron"] = sum / sum_baron
                    if "dragon" in file_path:
                        metric_group_highlights[-1]["dragon"] = sum / sum_dragon
                    if "inhibitor" in file_path:
                        metric_group_highlights[-1]["inhibitor"] = sum / sum_inhibitor
                    if "kills" in file_path:
                        metric_group_highlights[-1]["kills"] = sum / sum_kills
                    if "turrets" in file_path:
                        metric_group_highlights[-1]["turrets"] = sum / sum_turrets
                    if "ultimates" in file_path:
                        metric_group_highlights[-1]["ultimates"] = sum / sum_ultimates

    metric_group_matches = []
    for folder in segregated_matches.keys():
        for metric_group_path in segregated_matches[folder].keys():
            metric_group_matches.append({})
            for file_path in segregated_matches[folder][metric_group_path]:
                with open(base_path + "\\" + folder + "\\" + file_path) as file:
                    input_raw = file.readlines()
                    lines = []
                    for item in input_raw:
                        lines.append(float(item))
                    if "baron" in file_path:
                        metric_group_matches[-1]["baron"] = (lines)
                    if "dragon" in file_path:
                        metric_group_matches[-1]["dragon"] = (lines)
                    if "inhibitor" in file_path:
                        metric_group_matches[-1]["inhibitor"] = (lines)
                    if "kills" in file_path:
                        metric_group_matches[-1]["kills"] = (lines)
                    if "turrets" in file_path:
                        metric_group_matches[-1]["turrets"] = (lines)
                    if "ultimates" in file_path:
                        metric_group_matches[-1]["ultimates"] = (lines)

    setp = []
    for i in range(0, len(metric_group_highlights)):
        vals = list(metric_group_highlights[i].values())
        temp = []
        for j in range(0, len(vals)):
            temp.append(vals[j])
        setp.append(temp)
    array_highlights = np.array(setp, dtype=np.float64)


    return metric_group_matches, array_highlights

def shape_data(data):

    max_length = 0
    for unit in data:
        for item in unit.values():
            if len(item) > max_length:
                max_length = len(item)
        for item in unit.values():
            difference = max_length - len(item)
            for i in range(0, difference):
                item.append(0.0)

    setp = []
    for i in range(0, len(data)):
        vals = list(data[i].values())
        temp = []
        for k in range(0, len(vals[0])):
            for j in range(0, len(vals)):
                temp.append(vals[j][k])
            setp.append(temp)
            temp = []
    array = np.array(setp, dtype=np.float64)

    return array

def main():
    matches, highlights = load_data()

    #clean_highlights = shape_data(highlights)
    #clean_highlights.shape
    clean_labels = np.full((153, 1), 0.7)


    
    # Training data and labels
    #data = np.random.random((1000, 32))
    #labels = np.random.random((1000, 10))

    # Validation data and labels
    #val_data = np.random.random((1000, 32))
    #val_labels = np.random.random((1000, 10))

    # Model architecture
    model = keras.Sequential([
    keras.layers.Dense(64, activation='relu', input_shape=(6,)),
    keras.layers.Dense(64, activation='relu'),
    keras.layers.Dense(1, activation='softmax')])

    # Compiling
    model.compile(optimizer=tf.train.AdamOptimizer(0.01),
                  loss='mse',
                  metrics=["mae"])

    # Train model on data
    model.fit(highlights, clean_labels, epochs=200, batch_size=1)

'''
    # Another data form, datasets are scalable.
    dataset = tf.data.Dataset.from_tensor_slices((data, labels))
    dataset = dataset.batch(32).repeat()

    val_dataset = tf.data.Dataset.from_tensor_slices((val_data, val_labels))
    val_dataset = val_dataset.batch(32).repeat()

    # Validate model
    model.fit(dataset, epochs=10, steps_per_epoch=30,
              validation_data=val_dataset,
              validation_steps=3)
  '''

if __name__ == "__main__":
    main()