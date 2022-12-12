import face_recognition
from flask import Flask, request
from flask_restful import Resource, Api
import json
import sys
import os
import os.path
import random
import string
import datetime
from os import listdir
from os.path import isfile, join
import glob
from flask import send_file

app = Flask(__name__)
api = Api(app)
port = 5100

work_dir = os.getcwd()
faces_path = work_dir + "/faces/" 

class topic_tags(Resource):
    def post(self):

        onlyfiles = [f for f in listdir(faces_path) if isfile(join(faces_path, f))]
        
        known_faces = []

        for img_file in onlyfiles:
            known_faces.append(face_recognition.face_encodings(face_recognition.load_image_file(work_dir + "/faces/" + img_file))[0])


        img = request.files['img']
        

        basename = ''.join(random.choice(string.ascii_lowercase) for i in range(16))
        suffix = datetime.datetime.now().strftime("%y%m%d_%H%M%S")
        img_path = "_".join([basename, suffix]) + ".png"

        img.save(img_path)

        try:
            unknown_image = face_recognition.load_image_file(img_path)
            unknown_face_encoding = face_recognition.face_encodings(unknown_image)[0]
            results = face_recognition.compare_faces(known_faces, unknown_face_encoding)

            os.remove(img_path)
        except:
            os.remove(img_path)
            return str(-1)
        

        try:
            return str(onlyfiles[results.index(True)].replace(".png", ""))
        except:
            return str(-1)



class upload(Resource):
    def post(self):
        img = request.files['img']  
        img.save(faces_path + str(request.values['id']) + ".png") 
        
        return str([f for f in listdir(faces_path) if isfile(join(faces_path, f))])



class delete(Resource):
    def post(self):
        os.remove(faces_path + str(request.values['id']) + ".png") 
        
        return str([f for f in listdir(faces_path) if isfile(join(faces_path, f))])


class photo(Resource):
    def post(self):
        return send_file(faces_path + str(request.values['id']) + ".png", mimetype='image/gif') 


api.add_resource(topic_tags, '/')
api.add_resource(upload, '/upload')
api.add_resource(delete, '/delete')
api.add_resource(photo, '/photo')

app.run(host="0.0.0.0", port=port)
