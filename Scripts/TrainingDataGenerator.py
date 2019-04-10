import os
import cv2
import copy
import sys
import time
from ctypes import windll, Structure, c_long, byref
import json

def convert_frame_to_time(frame):
    return float(float(frame) / 30.0)


class POINT(Structure):
    _fields_ = [("x", c_long), ("y", c_long)]


def queryMousePosition():
    pt = POINT()
    windll.user32.GetCursorPos(byref(pt))
    return 1 - pt.y/1151


def main(match_to_process, score_output_path):
    score = []
    cap = cv2.VideoCapture(match_to_process)
    current_frame = 0
    while cap.isOpened():
        current_frame += 1
        ret, frame = cap.read()
        if ret is True:
            time_frame = convert_frame_to_time(current_frame - 10)
            cv2.imshow('view', frame)
            key = cv2.waitKey(0)
            if key & 0xFF == ord('w'):
                pos = queryMousePosition()
                if current_frame < 10:
                    continue
                score.append([time_frame, pos])
                print(pos)
                time.sleep(0.03)
                continue
        if ret is False:
            cap.release()
            with open(score_output_path, 'w') as output:
                json.dump(score, output)

if __name__ == "__main__":
    main('C:\\Users\\patin_000\\source\\repos\\DSP\\Broadcasts\\325392389\\match7.mp4', 'C:\\Users\\patin_000\\source\\repos\\DSP\\TensorflowData\\Unprocessed\\325392389_match7.txt')
    #main(sys.argv[1], sys.argv[2])
