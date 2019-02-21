import os
import cv2
from skimage.measure import compare_ssim


current_directory_path = os.path.dirname(os.path.abspath(__file__))

'''
Uses OCR technology (tesseract) to get the total kill count in each video frame in a supplied video file
It then will create a representation of kills over time
'''


def capture_kill_differences(cap, display_matched_frames=False):
    kills_over_time = []
    # Collecting video metadata for exporting to video
    fps = cap.get(cv2.CAP_PROP_FPS)
    video_width, video_height = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH)), int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
    number_of_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))

    prev_red_score = None
    current_frame = 1
    # Iterate over each frame
    while cap.isOpened():
        ret, frame = cap.read()
        if ret is True:
            # Process frame to make score white and background black
            frame = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            frame = cv2.threshold(frame, 50, 255, cv2.THRESH_TOZERO)[1]

            # Get region of blue teams score and red teams score
            blue_score = frame[2:30, int(video_width/2 - 25): int(video_width/2 - 1)]*2
            red_score = frame[2:30, int(video_width/2 + 8): int(video_width/2 + 25)]*2
            # Used to filter out scene transition effects that give false positives
            control_icon = frame[2:30, int(video_width/2 - 6): int(video_width/2 + 10)]*2

            # Starting condition
            if prev_red_score is None:
                prev_red_score = red_score
                prev_blue_score = blue_score
                prev_control_icon = control_icon
            else:
                # Get differences between the previous frame and current frame
                (diff_score_blue, diff_blue) = compare_ssim(prev_blue_score, blue_score, full=True)
                (diff_score_red, diff_red) = compare_ssim(prev_blue_score, blue_score, full=True)
                (diff_control_icon, diff_control) = compare_ssim(prev_control_icon, control_icon, full=True)

                # Check frame differences are below a certain threshold and the control is unchanged
                if diff_score_blue < 0.75 and diff_control_icon > 0.75:
                    # kill change registered on this frame
                    kills_over_time.append(current_frame)
                    if display_matched_frames is True:
                        cv2.imshow('curr different', blue_score)
                        cv2.imshow('prev different', prev_blue_score)
                        if cv2.waitKey(0) & 0xFF == ord('q'):
                            prev_red_score = red_score
                            prev_blue_score = blue_score
                            continue

                if diff_score_red < 0.75 and diff_control_icon > 0.75:
                    # kill change registered on this frame
                    kills_over_time.append(current_frame)
                    if display_matched_frames is True:
                        cv2.imshow('curr different', red_score)
                        cv2.imshow('prev different', prev_red_score)
                        if cv2.waitKey(0) & 0xFF == ord('q'):
                            prev_red_score = red_score
                            prev_blue_score = blue_score
                            continue
                prev_red_score = red_score
                prev_blue_score = blue_score
                prev_control_icon = control_icon
                current_frame += 1
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


def capture_ultimate_usage(cap, display_matched_frames=False):
    ult_usage_over_time = []

    # Draws a square of side length capture_region_offset
    capture_region_offset = 4

    # Locations of each teams champions ultimate status
    blue_team_positions = [(30, 70), (30, 116), (30, 162), (30, 208), (30, 254)]
    red_team_positions = [(817, 70), (817, 116), (817, 162), (817, 208), (817, 254)]

    # Collecting video metadata for exporting to video
    fps = cap.get(cv2.CAP_PROP_FPS)
    video_width, video_height = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH)), int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
    number_of_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))

    current_frame = 1
    starting_frame = True
    # Iterate over each frame
    while cap.isOpened():
        ret, frame = cap.read()
        if ret is True:

            # Used to filter out scene transition effects that give false positives
            control_icon = frame[2:30, int(video_width/2 - 6): int(video_width/2 + 10)]

            # Starting condition
            if starting_frame is True:
                prev_control_icon = control_icon
                starting_frame = False
            else:

                prev_control_icon = control_icon
                current_frame += 1
        # No more frames to process
        if ret is False:
            cap.release()

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

# Process number of kills in each highlight
highlight_kill_frames = []
for highlight in highlights:
    video_to_process = current_directory_path + '\\' + highlight
    # Load the video into cv2
    cap = cv2.VideoCapture(video_to_process)
    print('processing ' + highlight)
    kill_frames = capture_kill_differences(cap)
    highlight_kill_frames.append((highlight, kill_frames))

for highlight_name in highlight_kill_frames:
    print(highlight_name[0])
    print(len(highlight_name[1]))

# Process number of kills in each match
match_kill_frames = []
for match in matches:
    video_to_process = current_directory_path + '\\' + match
    # Load the video into cv2
    cap = cv2.VideoCapture(video_to_process)
    print('processing ' + match)
    kill_frames = capture_kill_differences(cap)
    match_kill_frames.append((match, kill_frames))

for match_name in match_kill_frames:
    print(match_name[0])
    print(len(match_name[1]))


