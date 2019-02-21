import os
from PIL import Image
import io
import cv2
from google.cloud import vision_v1p2beta1 as vision
from google.cloud.vision_v1p2beta1 import types

current_directory_path = os.path.dirname(os.path.abspath(__file__))
#TODO test this works on cloud instance
credential_path = current_directory_path + '\\visionapi.json'
os.environ["GOOGLE_APPLICATION_CREDENTIALS"] = credential_path

client = vision.ImageAnnotatorClient()

'''
Uses OCR technology (tesseract) to get the total kill count in each video frame in a supplied video file
It then will create a representation of kills over time
'''


def capture_kill_differences(cap):
    kills_over_time = []
    # Collecting video metadata for exporting to video
    fps = cap.get(cv2.CAP_PROP_FPS)
    video_width, video_height = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH)), int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
    number_of_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))

    # Iterate over each frame
    while cap.isOpened():
        ret, frame = cap.read()
        if ret is True:
            frame = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            # frame = cv2.threshold(frame, 20, 240, cv2.THRESH_TOZERO)[1]
            cropped_frame_blue_team = frame[2:30, int(video_width/2 - 25): int(video_width/2 - 2)]*2
            cropped_frame_red_team = frame[2:30, int(video_width/2 + 8): int(video_width/2 + 25)]*2
            # cv2.imshow('left', cropped_frame_left)
            # if cv2.waitKey(0) & 0xFF == ord('q'):
            #     break
            # cv2.imshow('right', cropped_frame_right)
            # if cv2.waitKey(0) & 0xFF == ord('q'):
            #     break

            # image_blue_team = types.text_annotation_pb2(content=cropped_frame_blue_team)
            # image_red_team = types.image(content=cropped_frame_red_team)

            cv2.imwrite('test.png', cropped_frame_blue_team)

            with io.open('test.png', 'rb') as testImage:
                image = vision.types.Image(content=testImage.read())
                response = client.text_detection(image=image)
            texts = response.text_annotations

            # cv2.imshow('left', cropped_frame_blue_team)
            # if cv2.waitKey(0) & 0xFF == ord('q'):
            #     break

            print('Texts:')

            for text in texts:
                print('\n"{}"'.format(text.description))

                vertices = (['({},{})'.format(vertex.x, vertex.y)
                             for vertex in text.bounding_poly.vertices])

                print('bounds: {}'.format(','.join(vertices)))

        # No more frames to process
        if ret is False:
            cap.release()
    return kills_over_time

'''
Uses openCV to locate the position of each team players position and the team player belongs to
it then will find the distance between opposing teams by averaging each teams player position
'''


def capture_distance_between_teams():
    distance_between_teams = []

    return distance_between_teams

'''
Uses openCV to capture the average colour of each player ultimate indicator
if a players ultimate indicator becomes desaturated between 2 frames then that player has used its ultimate
'''


def capture_ultimate_usage():
    ult_usage_over_time = []

    return ult_usage_over_time

# Get files in current directory
clips = os.listdir()



highlights = []
matches = []

# We filter out matches and clips
for clip in clips:
    if 'highlight' in clip and '.mp4' in clip:
        highlights.append(clip)
    if 'match' in clip and '.mp4' in clip:
        matches.append(clip)

for highlight in highlights:
    video_to_process = current_directory_path + '\\' + highlight
    # Load the video into cv2
    cap = cv2.VideoCapture(video_to_process)
    capture_kill_differences(cap)



