import os
import cv2
from skimage.measure import compare_ssim


current_directory_path = os.path.dirname(os.path.abspath(__file__))

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

    prev_red_score = None
    # Iterate over each frame
    while cap.isOpened():
        ret, frame = cap.read()
        if ret is True:
            frame = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            #frame = cv2.threshold(frame, 10, 255, cv2.THRESH_TOZERO)[1]
            blue_score = frame[2:30, int(video_width/2 - 25): int(video_width/2 - 1)]*2
            red_score = frame[2:30, int(video_width/2 + 8): int(video_width/2 + 25)]*2

            # TODO capture sword icon and check if within tolerance to filter out issues
            # TODO with false positives from scene transition effects
            # Starting condition
            if prev_red_score is None:
                prev_red_score = red_score
                prev_blue_score = blue_score
            else:
                (diff_score_blue, diff_blue) = compare_ssim(prev_blue_score, blue_score, full=True)
                (diff_score_red, diff_red) = compare_ssim(prev_blue_score, blue_score, full=True)

                if diff_score_blue > 0.90:
                    # cv2.imshow('curr same', blue_score)
                    # cv2.imshow('prev same', prev_blue_score)
                    # if cv2.waitKey(0) & 0xFF == ord('q'):
                    #     break
                    print ('similar')
                else:
                    print('different')
                    cv2.imshow('curr different', blue_score)
                    cv2.imshow('prev different', prev_blue_score)
                    if cv2.waitKey(0) & 0xFF == ord('q'):
                        prev_red_score = red_score
                        prev_blue_score = blue_score
                        continue
                prev_red_score = red_score
                prev_blue_score = blue_score



            # cv2.imshow('left', cropped_frame_left)
            # if cv2.waitKey(0) & 0xFF == ord('q'):
            #     break
            # cv2.imshow('right', cropped_frame_right)
            # if cv2.waitKey(0) & 0xFF == ord('q'):
            #     break

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
    print ('processing ' + highlight)
    capture_kill_differences(cap)



