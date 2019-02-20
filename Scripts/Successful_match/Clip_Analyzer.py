import os
import cv2

# Get files in current directory
clips = os.listdir()

current_directory_path = os.path.dirname(os.path.abspath(__file__))

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


'''
Uses OCR technology to get the total kill count in each video frame in a supplied video file
It then will create a representation of kills over time
'''


def capture_kill_differences():
    kills_over_time = []
    # Collecting video metadata for exporting to video
    fps = cap.get(cv2.CAP_PROP_FPS)
    video_width, video_height = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH)), int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
    number_of_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))

    # Iterate over each frame
    while cap.isOpened():
        ret, frame = cap.read()
        if ret is True:

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

