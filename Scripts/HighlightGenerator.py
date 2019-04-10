import cv2
import datetime
import os
import time
import csv
import sys

'''
Takes a video and a template image and filters out any frames that match against the template image.
If there is a gap between filtered frames of length seconds_until_timeout then the current output video is saved
and a new video output is started. This will segregate videos into separate matches and highlights.
'''


def filter_video_by_template(video_to_process, output_path, time_start, time_end):
    default_output = 'output.mp4'

    # Load the video into cv2
    cap = cv2.VideoCapture(video_to_process)



    # Collecting video metadata for exporting to video
    fps = cap.get(cv2.CAP_PROP_FPS)
    video_width, video_height = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH)), int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))

    starting_frame = convert_seconds_to_frame(time_start, fps)
    # You can seek forward n frames in the video with this
    cap.set(cv2.CAP_PROP_POS_FRAMES, starting_frame)

    # We get this metadata to display current progress
    number_of_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))

    # We feed in 0x00000021 as fourcc doesnt map to mp4 correctly
    # fourcc = cv2.VideoWriter_fourcc(*'mp4v')

    # Our filtered output video
    out = cv2.VideoWriter(output_path, 0x00000021, fps, (video_width, video_height))

    # Iterate through each frame, and write to out each frame which matches the template
    current_frame = starting_frame
    while cap.isOpened():
        ret, frame = cap.read()
        if ret is True:
            out.write(frame)
        if ret is False:
            cap.release()
            out.release()
        if convert_frame_to_seconds(current_frame, fps) >= time_end:
            cap.release()
            out.release()
        current_frame += 1


def convert_frame_to_seconds(frame, fps):
    return frame / fps


def convert_seconds_to_frame(seconds, fps):
    return int((float(seconds)) * (float(fps)))


def main(video_to_process, output_path, time_start, time_end):
    filter_video_by_template(
        video_to_process=video_to_process, output_path=output_path, time_start=float(time_start), time_end=float(time_end))


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4])


