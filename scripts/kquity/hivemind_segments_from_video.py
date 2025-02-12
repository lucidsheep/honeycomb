import editdistance
import imageio
import cv2
import time
import pytesseract


def main2():
    video = imageio.get_reader('./League Night 2⧸13⧸23.mkv', 'ffmpeg')
    start = time.time()
    print(video.get_meta_data())
    for frame_idx in range(0, 100000000, 180):
        detect = cv2.QRCodeDetector()
        frame = video.get_data(frame_idx)
        value, points, straight_qrcode = detect.detectAndDecode(frame)
        if (frame_idx % 1800) == 0:
            print('progress', frame_idx, frame_idx / 3600, time.time() - start)
        if value:
            print('capture hivemind', frame_idx, value)
        if is_kq_start_screen_ocr_text(extract_text(frame)):
            print('capture kq start', frame_idx)


def extract_text(img):
    img = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    img = img[220:img.shape[0], 0:1540]


    th, img = cv2.threshold(img, 250, 255, cv2.THRESH_BINARY)

#    cv2.imshow('img', img)
 #   cv2.waitKey(0)

 #   cv2.destroyAllWindows()

    img = cv2.resize(img, (img.shape[1] // 3, img.shape[0] // 3), interpolation=cv2.INTER_AREA)

    texts = pytesseract.image_to_data(img, lang='eng', output_type=pytesseract.Output.DICT)['text']

    return [t.lower() for t in texts if t.strip()]


def single_pool_match(text, cand_pool):
    for cand in cand_pool:
        if editdistance.eval(text, cand.lower()) <= 1:
            return True
    return False


def multi_pool_counts(texts, cand_pools):
    count = 0
    for text in texts:
        for cand_pool in cand_pools:
            if single_pool_match(text, cand_pool):
                count += 1
                break

    return count


def is_kq_start_screen_ocr_text(texts):
    dusk_twilight_texts = [['SNAIL'], ['PEHIND', 'BEHIND'], ['Ap0ve', 'above', '4pove', 'apove', 'ApoYE']]
    day_night_texts = [['MILITARY'], ['ECONOMIC'], ['SNAIL']]

    return multi_pool_counts(texts, dusk_twilight_texts) >= 2 or multi_pool_counts(texts, day_night_texts) >= 2


def main():
    video = imageio.get_reader('./League Night 2⧸13⧸23.mkv', 'ffmpeg')

    # dusk 135314  'apove', 'PEHIND', 'SNAIL'  '4pove', '=', 'BEHIND', 'SNAIL' 'ApOve'
    # night  215306 # 'MILITARY', 'ECONOMIC', 'SNAIL'
    # day 86338  207790 real # 'MILITARY', 'ECONOMIC', 'SNAIL'
    # twilight 899400 snail
    img = video.get_data(899500)
    kq_texts = extract_text(img)
    print('is_kq_start_screen_ocr_text', is_kq_start_screen_ocr_text(kq_texts), kq_texts)


def main3():
    def get_segmented_frames(video, segmented_frame_indexes):
        """Gets the segmented frames from the given video and segmented frame indexes.

        Parameters
        ----------
        video : cv2.VideoCapture
            The video to get the segmented frames from.
        segmented_frame_indexes : list
            A list of pairs of frame indexes, where the first element of each pair is the start index and the second element is the end index.

        Returns
        -------
        list
            A list of lists of segmented frames.
        """
        segs = []
        for start, end in segmented_frame_indexes:
            this_capture = []
            video.set(cv2.CAP_PROP_POS_FRAMES, start)
            for _ in range(start, end):
                this_capture.append(video.read())
            segs.append(this_capture)
        return segs

    def write_segmented_videos(segmented_frames, output_dir):
        """Writes the segmented frames to the given output directory.

        Parameters
        ----------
        segmented_frames : list
            A list of segmented frames.
        output_dir : str
            The directory to write the segmented videos to.
        """
        for idx, frame_list in enumerate(segmented_frames):

            # Write the frame to a video.
            #codec = cv2.VideoWriter_fourcc('a','v','c','1')
            codec = cv2.VideoWriter_fourcc('M', 'J', 'P', 'G')
            h, w = frame_list[0][1].shape[:2]
            output_video = cv2.VideoWriter(f'{output_dir}/{idx}.',
                                           codec, 60, (w, h))
            for frame in frame_list:
                output_video.write(frame[1])
            output_video.release()

    # Get the video and segmented frame indexes.
    video = cv2.VideoCapture('League Night 2⧸13⧸23.mkv')
    segmented_frame_indexes = [(0, 100), (101, 200), (201, 300)]

    segmented_frames = get_segmented_frames(video, segmented_frame_indexes)
    print('len sgemented frames is', len(segmented_frames))
    write_segmented_videos(segmented_frames, 'segmented_videos')
    print('done')

if __name__ == '__main__':
    main()