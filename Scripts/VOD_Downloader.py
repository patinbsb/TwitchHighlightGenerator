from streamlink import Streamlink
session = Streamlink()
session.set_option('http-headers', 'Authorization=OAuth dfa3898c4a5czuud9pworqh0eoaj8x')
session.set_option('output', 'Authorization=OAuth dfa3898c4a5czuud9pworqh0eoaj8x')
streams = session.streams("https://api.twitch.tv/riotgames/v/327962589")
video = streams["480p"]
output = video.open()
with open("test.mp4", 'wb') as vid_out:
    while output:
        vid_out.write(output.read(1024))