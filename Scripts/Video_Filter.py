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


def filter_video_by_template(video_to_process, output_path, filter_template='C:\\Twitch VODs\\template.png',
                             filter_threshold=0.9, starting_frame=1, frames_to_skip=15,
                             convert_to_greyscale=False, print_progress=True,
                             display_matched_frames=False, seconds_until_timeout=120, seconds_minimum_match_length=600
                             ):
    default_output = 'output.mp4'

    # Load the video into cv2
    cap = cv2.VideoCapture(video_to_process)

    # You can seek forward n frames in the video with this
    cap.set(cv2.CAP_PROP_POS_FRAMES, starting_frame)

    # We load in our template image as greyscale, any frames that do not contain this image are filtered out
    template = cv2.imread(filter_template, 0)

    # Collecting video metadata for exporting to video
    fps = cap.get(cv2.CAP_PROP_FPS)
    video_width, video_height = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH)), int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))

    # Checking input parameters are legal
    if frames_to_skip > seconds_until_timeout * fps:
        raise ValueError("frames_to_skip is too large")

    # We get this metadata to display current progress
    number_of_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))

    # We feed in 0x00000021 as fourcc doesnt map to mp4 correctly
    # fourcc = cv2.VideoWriter_fourcc(*'mp4v')

    # Our filtered output video
    out = cv2.VideoWriter((output_path + default_output), 0x00000021, fps, (video_width, video_height))

    # Iterate through each frame, and write to out each frame which matches the template
    current_frame = starting_frame
    currently_matching = False

    highlight_count = 1
    match_count = 1

    audio_segment_start = None

    stopwatch_start = time.time()
    # A list of start and end frame tuples that will represent the audio sections to capture in a match/highlight
    matched_ranges = []
    while cap.isOpened():
        ret, frame = cap.read()
        if ret is True and (current_frame % frames_to_skip == 0):
            grey_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            # We check if the template is in the frame
            res = cv2.matchTemplate(grey_frame, template, cv2.TM_CCOEFF_NORMED)

            # Extract the best match to the template
            _, max_threshold, _, max_location = cv2.minMaxLoc(res)
            flag = False
            if max_threshold >= filter_threshold:
                flag = True

            # If frame is good, we write the filtered frame to out
            if flag:
                if currently_matching is False:
                    start_match_frame = current_frame
                    previous_matched_frame = current_frame
                    audio_segment_start = current_frame
                    currently_matching = True

                # The render has not found a matched frame within the seconds_until_timeout range
                if currently_matching:
                    # We check if there is a skip in the matching.
                    if current_frame - previous_matched_frame != frames_to_skip \
                            and current_frame - previous_matched_frame != 0:
                        matched_ranges.append([convert_frame_to_seconds(audio_segment_start, fps),
                                                  convert_frame_to_seconds(previous_matched_frame, fps)])
                        audio_segment_start = current_frame

                    # The render has discovered a highlight video (template match length < seconds_minimum_match_length)
                    if current_frame - seconds_until_timeout * fps > previous_matched_frame \
                            and matched_ranges_to_total_seconds(matched_ranges) < seconds_minimum_match_length:
                        # End current video
                        out.release()
                        # Rename output video to highlight + highlight_count
                        filename = 'highlight' + str(highlight_count)
                        os.rename((output_path + default_output), output_path + filename + '.mp4')
                        # Write out audio frames for future audio/video merging
                        write_highlight_or_match_audio_segment_to_file(output_path + filename, matched_ranges)
                        matched_ranges.clear()
                        if print_progress:
                            print(filename + ' video created', flush=True)
                        highlight_count += 1
                        # Start new video render
                        out = cv2.VideoWriter((output_path + default_output), 0x00000021, fps, (video_width, video_height))
                        currently_matching = False

                    # The render has discovered a match video (template match length > seconds_minimum_match_length)
                    elif current_frame - seconds_until_timeout * fps > previous_matched_frame \
                            and matched_ranges_to_total_seconds(matched_ranges) > seconds_minimum_match_length:
                        # End current video
                        out.release()
                        # Rename output video to match + match_count
                        filename = 'match' + str(match_count)
                        os.rename((output_path + default_output), output_path + filename + '.mp4')
                        # Write out audio frames for future audio/video merging
                        write_highlight_or_match_audio_segment_to_file(output_path + filename, matched_ranges)
                        matched_ranges.clear()
                        if print_progress:
                            print('match' + str(match_count) + ' video created', flush=True)
                        match_count += 1
                        # Start new video render
                        out = cv2.VideoWriter((output_path + default_output), 0x00000021, fps, (video_width, video_height))
                        currently_matching = False

                previous_matched_frame = current_frame
                # For debugging template matching
                if display_matched_frames:
                    template_width, template_height = template.shape[::-1]
                    cv2.rectangle(grey_frame, max_location,
                                  (max_location[0] + template_width, max_location[1] + template_height),
                                  (255, 255, 255), 2)
                    cv2.imshow('Detected', grey_frame)
                    if cv2.waitKey(0) & 0xFF == ord('q'):
                        break
                if convert_to_greyscale:
                    out.write(grey_frame)
                else:
                    out.write(frame)

            # For updating user on progress of render
            prev_progress = "{0:.0f}".format(((current_frame - frames_to_skip) / number_of_frames) * 100)
            current_progress = "{0:.0f}".format((current_frame / number_of_frames) * 100)
            if prev_progress != current_progress and print_progress:
                print('Current time: ' + str(datetime.timedelta(seconds=int(current_frame / 30))) +
                      ' / Total time: ' + str(datetime.timedelta(seconds=int(number_of_frames / 30))) +
                      ' | ' + str("{0:.0f}".format((current_frame / number_of_frames) * 100)) + '%' +
                      ' | Time elapsed: ' + str("{0:.0f}".format((time.time() - stopwatch_start)/60)) + ' minutes', flush=True)

        current_frame += 1

        # No more frames to process
        if ret is False:
            cap.release()
            out.release()
            if matched_ranges_to_total_seconds(matched_ranges) < seconds_minimum_match_length:
                # Rename output video to highlight + highlight_count
                filename = 'highlight' + str(highlight_count)
            else:
                # Rename output video to highlight + highlight_count
                filename = 'match' + str(match_count)

            os.rename((output_path + default_output), output_path + filename + '.mp4')
            # Write out audio frames for future audio/video merging
            if audio_segment_start is not None:
                matched_ranges.append([convert_frame_to_seconds(audio_segment_start, fps),
                                          convert_frame_to_seconds(previous_matched_frame, fps)])
                write_highlight_or_match_audio_segment_to_file(output_path + filename, matched_ranges)
                matched_ranges.clear()
            if print_progress:
                print(filename + ' video created', flush=True)
    cv2.destroyAllWindows()
    return 0


def write_highlight_or_match_audio_segment_to_file(filename, audio_seconds):
    with open(filename + '.csv', 'w', newline='') as csvfile:
        writer = csv.writer(csvfile)
        writer.writerows(audio_seconds)


def convert_frame_to_seconds(frame, fps):
    return frame / fps


def matched_ranges_to_total_seconds(matched_ranges):
    output = 0
    for time_range in matched_ranges:
        output += time_range[1] - time_range[0]
    return output

def string_to_bool(input):
    if input == 'True':
        return True
    else:
        return False


def main(video_to_process, output_path, filter_template,
         filter_threshold, starting_frame, frames_to_skip,
         convert_to_greyscale, seconds_until_timeout, seconds_minimum_match_length):
    filter_video_by_template(
        video_to_process=video_to_process, output_path=output_path, filter_template=filter_template,
        filter_threshold=filter_threshold, starting_frame=starting_frame, frames_to_skip=frames_to_skip,
        convert_to_greyscale=convert_to_greyscale, seconds_until_timeout=seconds_until_timeout,
        seconds_minimum_match_length=seconds_minimum_match_length)


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2], sys.argv[3],
         float(sys.argv[4]), int(sys.argv[5]), int(sys.argv[6]),
         string_to_bool(sys.argv[7]), int(sys.argv[8]), int(sys.argv[9]))


