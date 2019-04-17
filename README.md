# TwitchHighlightGenerator
Automatically generates highlight reels from twitch vods

Installation instructions.

1. Extract folder from zip file to a location on your C drive.
2. Run Create_DB.sql to setup the local sql environment
3. install conda https://www.anaconda.com/distribution/
4. in a admin command prompt run the following commands:
    a. conda install python=3.6
    b. conda create --name PythonCPU
    c. activate PythonCPU
    d. conda install -c anaconda keras
    (optional: if you set both interpreter paths to be equal in 2.a)
    e. conda install -c anaconda opencv-python
       conda install -c anaconda opencv-contrib-python
       conda install -c anaconda scikit-image
       conda install -c anaconda numpy
    (optional: if you DID NOT set both interpreter paths to be equal in 2.a)
    f.  pip install opencv-python
        pip install opencv-contrib-python
        pip install scikit-image
        pip install numpy
    
5. Open DSPSubmission\TwitchBackend\HighlightGenerator\HighlightGenerator\App.config and:
    a. set PythonInterpreterPath to the location of the local python exe (or same as Conda python exe)
    b. set TensorflowPythonInterpreterPath to the location of the local python exe (C:\Users\<username>\Anaconda3\envs\PythonCPU\python.exe)
    c. set MySqlConnection string with the user id and password of a user with read/write privileges on the dsp DB

6. Load solution file at DSPSubmission\TwitchBackend\HighlightGenerator\HighlightGenerator.sln and run.