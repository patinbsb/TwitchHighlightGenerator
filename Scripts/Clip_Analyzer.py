import os
import cv2
import copy
from skimage.measure import compare_ssim
import sys

current_directory_path = os.path.dirname(os.path.abspath(__file__))

'''
Uses skimage to capture changes in the number of kills for each team
It will output a list of frames where a kill occurred.
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
                    kills_over_time.append(convert_frame_to_time(current_frame))
                    if display_matched_frames is True:
                        cv2.imshow('curr different', blue_score)
                        cv2.imshow('prev different', prev_blue_score)
                        if cv2.waitKey(0) & 0xFF == ord('q'):
                            prev_red_score = red_score
                            prev_blue_score = blue_score
                            continue

                if diff_score_red < 0.75 and diff_control_icon > 0.75:
                    # kill change registered on this frame
                    kills_over_time.append(convert_frame_to_time(current_frame))
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
Uses openCV to capture the average colour of each player ultimate indicator
if a players ultimate indicator becomes desaturated between 2 frames then that player has used its ultimate
outputs a list of frames where an ultimate use occurred and the team + player that used its ultimate.
filters out ultimate usages where the number of ultimates used was over 3 to reduce noise
also filters out team members that have a excessively large amount of ultimate usages 
'''


def capture_ultimate_usage(cap, display_matched_frames=False):
    ult_usage_over_time = []

    # Draws a square of side length offset
    offset = 4

    # Locations of each teams champions ultimate status
    blue_team_positions = [(30, 70), (30, 116), (30, 162), (30, 208), (30, 254)]
    red_team_positions = [(817, 70), (817, 116), (817, 162), (817, 208), (817, 254)]

    # Collecting video metadata for exporting to video
    fps = cap.get(cv2.CAP_PROP_FPS)
    video_width, video_height = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH)), int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
    number_of_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))

    current_frame = 1
    starting_frame = True
    prev_red_team_frames = []
    prev_blue_team_frames = []
    team_position = {}
    blacklist = []
    # Iterate over each frame
    while cap.isOpened():
        ret, frame = cap.read()
        if ret is True:
            blue_team_frames = []
            for position in blue_team_positions:
                x, y = position[0], position[1]
                blue_team_frames.append(frame[y:y + offset, x:x + offset])

            red_team_frames = []
            for position in red_team_positions:
                x, y = position[0], position[1]
                red_team_frames.append(frame[y:y + offset, x:x + offset])

            # Used to filter out scene transition effects that give false positives
            control_icon = frame[2:30, int(video_width/2 - 6): int(video_width/2 + 10)]

            # Starting condition
            if starting_frame is True:
                prev_control_icon = control_icon
                prev_red_team_frames = copy.deepcopy(red_team_frames)
                prev_blue_team_frames = copy.deepcopy(blue_team_frames)
                prev_frame = frame
                starting_frame = False
            else:
                mean_frame = cv2.mean(cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY))
                mean_prev_frame = cv2.mean(cv2.cvtColor(prev_frame, cv2.COLOR_BGR2GRAY))
                mean_brightness_change = abs(mean_frame[0] - mean_prev_frame[0])



                team = 'red'
                position = 1
                ult_use_positions = []
                for curr_ult, prev_ult in zip(red_team_frames, prev_red_team_frames):
                    mean_ult_frame = cv2.mean(cv2.cvtColor(curr_ult, cv2.COLOR_BGR2GRAY))
                    mean_prev_ult_frame = cv2.mean(cv2.cvtColor(prev_ult, cv2.COLOR_BGR2GRAY))

                    if mean_prev_ult_frame[0] - mean_ult_frame[0] > mean_brightness_change + 20:
                        if display_matched_frames:
                            print('found brightness change: ' + str(abs(mean_prev_ult_frame[0] - mean_ult_frame[0]))
                                + ' | red team, position: ' + str(position) + ' | Avg change: '
                                  + str(mean_brightness_change))
                            cv2.imshow('curr', frame)
                            cv2.imshow('prev', prev_frame)
                            if cv2.waitKey(0) & 0xFF == ord('q'):
                                prev_red_team_frames = copy.deepcopy(red_team_frames)
                                prev_blue_team_frames = copy.deepcopy(blue_team_frames)
                                prev_control_icon = control_icon
                                prev_frame = frame
                                position += 1
                                continue

                        if (team, position) in team_position:
                            result = team_position[(team, position)]
                            if abs(result[0] - current_frame) < 100:
                                if result[1] + 1 > 6 and (team, position) not in blacklist:
                                    blacklist.append((team, position))
                                    ult_use_positions[:] = [x for x in ult_use_positions if x != (team, position)]

                                team_position[(team, position)] = (current_frame, result[1] + 1)
                            else:
                                team_position[(team, position)] = (current_frame, result[1])
                        else:
                            team_position[(team, position)] = (current_frame, 0)
                        if (team, position) not in blacklist:
                            ult_use_positions.append([team, position])
                    position += 1

                position = 1
                team = 'blue'
                for curr_ult, prev_ult in zip(blue_team_frames, prev_blue_team_frames):
                    mean_ult_frame = cv2.mean(cv2.cvtColor(curr_ult, cv2.COLOR_BGR2GRAY))
                    mean_prev_ult_frame = cv2.mean(cv2.cvtColor(prev_ult, cv2.COLOR_BGR2GRAY))

                    if mean_prev_ult_frame[0] - mean_ult_frame[0] > mean_brightness_change + 20:
                        if display_matched_frames:
                            print('found brightness change: ' + str(abs(mean_prev_ult_frame[0] - mean_ult_frame[0]))
                                  + ' | blue team, position: ' + str(position) + ' | Avg change: ' +
                                  str(mean_brightness_change))
                            cv2.imshow('curr', frame)
                            cv2.imshow('prev', prev_frame)
                            if cv2.waitKey(0) & 0xFF == ord('q'):
                                prev_red_team_frames = copy.deepcopy(red_team_frames)
                                prev_blue_team_frames = copy.deepcopy(blue_team_frames)
                                prev_control_icon = control_icon
                                prev_frame = frame
                                position += 1
                                continue

                        if (team, position) in team_position:
                            result = team_position[(team, position)]
                            if abs(result[0] - current_frame) < 100:
                                if result[1] + 1 > 6 and (team, position) not in blacklist:
                                    blacklist.append((team, position))
                                    ult_use_positions[:] = [x for x in ult_use_positions if x != (team, position)]

                                team_position[(team, position)] = (current_frame, result[1] + 1)
                            else:
                                team_position[(team, position)] = (current_frame, result[1])
                        else:
                            team_position[(team, position)] = (current_frame, 0)
                        if (team, position) not in blacklist:
                            ult_use_positions.append([team, position])
                    position += 1

                # Filter out radical ult usage
                if len(ult_use_positions) < 3 and len(ult_use_positions) > 0:

                    ult_usage_over_time.append([convert_frame_to_time(current_frame), ult_use_positions])

                prev_red_team_frames = copy.deepcopy(red_team_frames)
                prev_blue_team_frames = copy.deepcopy(blue_team_frames)
                prev_control_icon = control_icon
                prev_frame = frame
                current_frame += 1
        # No more frames to process
        if ret is False:
            cap.release()

    return ult_usage_over_time


def capture_objectives(cap, display_matched_frames = False):
    blue_turret_path = current_directory_path + "\\turret_blue.png"
    baron_path = current_directory_path + "\\baron.png"
    dragon_path = current_directory_path + "\\dragon.png"
    dragon_path2 = current_directory_path + "\\dragon2.png"
    dragon_path3 = current_directory_path + "\\dragon3.png"
    inhibitor_path = current_directory_path + "\\inhibitor.png"
    blue_turret_template = cv2.imread(blue_turret_path, 0)
    baron_template = cv2.imread(baron_path, 0)
    dragon_template = cv2.imread(dragon_path, 0)
    dragon2_template = cv2.imread(dragon_path2, 0)
    dragon3_template = cv2.imread(dragon_path3, 0)
    inhibitor_template = cv2.imread(inhibitor_path, 0)

    target_threshold = 0.90
    turret_frames = []
    baron_frames = []
    dragon_frames = []
    inhibitor_frames = []
    current_frame = 1
    turret_found = False
    baron_found = False
    dragon_found = False
    inhibitor_found = False
    turret_timer = 30
    baron_timer = 30
    dragon_timer = 30
    inhibitor_timer = 30
    while cap.isOpened():
        ret, frame = cap.read()
        if turret_found and turret_timer > 0:
            turret_timer -= 1
        else:
            turret_found = False
            turret_timer = 30

        if baron_found and baron_timer > 0:
            baron_timer -= 1
        else:
            baron_found = False
            baron_timer = 30

        if dragon_found and dragon_timer > 0:
            dragon_timer -= 1
        else:
            dragon_found = False
            dragon_timer = 30

        if inhibitor_found and inhibitor_timer > 0:
            inhibitor_timer -= 1
        else:
            inhibitor_found = False
            inhibitor_timer = 30
        if ret is True:
            grey_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            grey_frame = grey_frame[307:346, 713:755]
            res_turret = cv2.matchTemplate(grey_frame, blue_turret_template, cv2.TM_CCOEFF_NORMED)
            res_baron = cv2.matchTemplate(grey_frame, baron_template, cv2.TM_CCOEFF_NORMED)
            res_dragon = cv2.matchTemplate(grey_frame, dragon_template, cv2.TM_CCOEFF_NORMED)
            res_dragon2 = cv2.matchTemplate(grey_frame, dragon2_template, cv2.TM_CCOEFF_NORMED)
            res_dragon3 = cv2.matchTemplate(grey_frame, dragon3_template, cv2.TM_CCOEFF_NORMED)
            res_inhibitor = cv2.matchTemplate(grey_frame, inhibitor_template, cv2.TM_CCOEFF_NORMED)

            _, max_threshold, _, max_location = cv2.minMaxLoc(res_turret)
            if max_threshold >= target_threshold and turret_found is False:
                turret_found = True
                turret_frames.append(convert_frame_to_time(current_frame))
                if display_matched_frames:
                    cv2.imshow('turret', frame)
                    if cv2.waitKey(0) & 0xFF == ord('q'):
                        continue

            _, max_threshold, _, max_location = cv2.minMaxLoc(res_baron)
            if max_threshold >= target_threshold and baron_found is False:
                baron_found = True
                baron_frames.append(convert_frame_to_time(current_frame))
                if display_matched_frames:
                    cv2.imshow('baron', frame)
                    if cv2.waitKey(0) & 0xFF == ord('q'):
                        continue

            _, max_threshold, _, max_location = cv2.minMaxLoc(res_dragon)
            _, max_threshold2, _, max_location = cv2.minMaxLoc(res_dragon2)
            _, max_threshold3, _, max_location = cv2.minMaxLoc(res_dragon3)
            if (max_threshold >= target_threshold or max_threshold2 >= target_threshold or max_threshold3 >= target_threshold) and dragon_found is False:
                dragon_found = True
                dragon_frames.append(convert_frame_to_time(current_frame))
                if display_matched_frames:
                    cv2.imshow('dragon', frame)
                    if cv2.waitKey(0) & 0xFF == ord('q'):
                        continue

            _, max_threshold, _, max_location = cv2.minMaxLoc(res_inhibitor)
            if max_threshold >= target_threshold and inhibitor_found is False:
                inhibitor_found = True
                inhibitor_frames.append(convert_frame_to_time(current_frame))
                if display_matched_frames:
                    cv2.imshow('inhibitor', frame)
                    if cv2.waitKey(0) & 0xFF == ord('q'):
                        continue
        if ret is False:
            cap.release()
            return turret_frames, baron_frames, dragon_frames, inhibitor_frames;
        current_frame += 1

def convert_frame_to_time(frame):
    return float(float(frame) / 30.0)


def main(match_to_process, kill_path, ultimate_path, turret_path, baron_path, dragon_path, inhibitor_path):
    match_ultimate_frames = []
    matched_kill_frames = []
    matched_turret_frames = []
    matched_baron_frames = []
    matched_dragon_frames = []
    matched_inhibitor_frames = []

    cap = cv2.VideoCapture(match_to_process)
    matched_turret_frames, matched_baron_frames, matched_dragon_frames, matched_inhibitor_frames = capture_objectives(cap)

    cap = cv2.VideoCapture(match_to_process)
    match_ultimate_frames = capture_ultimate_usage(cap)

    cap = cv2.VideoCapture(match_to_process)
    matched_kill_frames = capture_kill_differences(cap)

    with open(kill_path, 'w') as kill_json:
        for kill in matched_kill_frames:
            kill_json.write(str(kill) + "\n")

    with open(ultimate_path, 'w') as ultimate_json:
        for ultimate in match_ultimate_frames:
            ultimate_json.write(str(ultimate[0]) + "\n")

    with open(turret_path, 'w') as turret_file:
        for turret in matched_turret_frames:
            turret_file.write(str(turret) + "\n")

    with open(baron_path, 'w') as baron_file:
        for baron in matched_baron_frames:
            baron_file.write(str(baron) + "\n")

    with open(dragon_path, 'w') as dragon_file:
        for dragon in matched_dragon_frames:
            dragon_file.write(str(dragon) + "\n")

    with open(inhibitor_path, 'w') as inhibitor_file:
        for inhibitor in matched_inhibitor_frames:
            inhibitor_file.write(str(inhibitor) + "\n")



if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4], sys.argv[5], sys.argv[6], sys.argv[7])


#TODO grade ultimate score on a scale of sum(player ults) = sum 1/(total_player_ults) = 1.0
#TODO so if player ults 10 times, each ult has a score of 0.1 (soft cap? minimum/max?)
#TODO grade kill value from 1.0 * kill at time=0 to 0.8 * kill at time=end_of_match
