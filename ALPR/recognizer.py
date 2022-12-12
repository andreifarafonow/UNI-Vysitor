import ultimateAlprSdk
import argparse
import json
import os.path
from flask import Flask, request
from flask_restful import Resource, Api
import sys
import os
import random
import string
import datetime

app = Flask(__name__)
api = Api(app)
port = 5100

class topic_tags(Resource):
    def post(self):

        img = request.files['img']
        

        basename = ''.join(random.choice(string.ascii_lowercase) for i in range(16))
        suffix = datetime.datetime.now().strftime("%y%m%d_%H%M%S")
        img_path = "_".join([basename, suffix]) + ".png"

        img.save(img_path)

        if not os.path.isfile(img_path):
            raise OSError(TAG + "File doesn't exist: %s" % img_path)

        image, imageType = load_pil_image(img_path)
        width, height = image.size

        response = ultimateAlprSdk.UltAlprSdkEngine_process(
                        imageType,
                        image.tobytes(), # type(x) == bytes
                        width,
                        height,
                        0, # stride
                        1 # exifOrientation (already rotated in load_image -> use default value: 1)
                    ).json()

        os.remove(img_path)

        return response



api.add_resource(topic_tags, '/')

TAG = "[PythonRecognizer] "

# Defines the default JSON configuration. More information at https://www.doubango.org/SDKs/anpr/docs/Configuration_options.html
JSON_CONFIG = {
    "debug_level": "info",
    "debug_write_input_image_enabled": False,
    "debug_internal_data_path": ".",
    
    "num_threads": -1,
    "gpgpu_enabled": True,
    "max_latency": -1,

    "klass_vcr_gamma": 1.5,
    
    "detect_roi": [0, 0, 0, 0],
    "detect_minscore": 0.1,

    "car_noplate_detect_min_score": 0.8,
    
    "pyramidal_search_enabled": True,
    "pyramidal_search_sensitivity": 0.28,
    "pyramidal_search_minscore": 0.3,
    "pyramidal_search_min_image_size_inpixels": 800,
    
    "recogn_rectify_enabled": True,
    "recogn_minscore": 0.3,
    "recogn_score_type": "min"
}

IMAGE_TYPES_MAPPING = { 
        'RGB': ultimateAlprSdk.ULTALPR_SDK_IMAGE_TYPE_RGB24,
        'RGBA': ultimateAlprSdk.ULTALPR_SDK_IMAGE_TYPE_RGBA32,
        'L': ultimateAlprSdk.ULTALPR_SDK_IMAGE_TYPE_Y
}

# Load image
def load_pil_image(path):
    from PIL import Image, ExifTags, ImageOps
    import traceback
    pil_image = Image.open(path)
    img_exif = pil_image.getexif()
    ret = {}
    orientation  = 1
    try:
        if img_exif:
            for tag, value in img_exif.items():
                decoded = ExifTags.TAGS.get(tag, tag)
                ret[decoded] = value
            orientation  = ret["Orientation"]
    except Exception as e:
        print(TAG + "An exception occurred: {}".format(e))
        traceback.print_exc()

    if orientation > 1:
        pil_image = ImageOps.exif_transpose(pil_image)

    if pil_image.mode in IMAGE_TYPES_MAPPING:
        imageType = IMAGE_TYPES_MAPPING[pil_image.mode]
    else:
        raise ValueError(TAG + "Invalid mode: %s" % pil_image.mode)

    return pil_image, imageType

# Check result
def checkResult(operation, result):
    if not result.isOK():
        print(TAG + operation + ": failed -> " + result.phrase())
        assert False
    else:
        print(TAG + operation + ": OK -> " + result.json())

# Entry point
if __name__ == "__main__":
    
    data = '{"image" : "img.png", "assets"  : "assets", "klass_lpci_enabled" :  true ,"klass_vcr_enabled" :  true,"klass_vmmr_enabled" :  true}'
    args = json.loads(data)

    # Update JSON options using values from the command args
    JSON_CONFIG["assets_folder"] = args["assets"]
    JSON_CONFIG["klass_lpci_enabled"] = (args['klass_lpci_enabled'] == True)
    JSON_CONFIG["klass_vcr_enabled"] = (args['klass_vcr_enabled'] == True)
    JSON_CONFIG["klass_vmmr_enabled"] = (args['klass_vmmr_enabled'] == True)


    # Initialize the engine
    checkResult("Init", 
                ultimateAlprSdk.UltAlprSdkEngine_init(json.dumps(JSON_CONFIG))
               )

    app.run(host="0.0.0.0", port=port)

    







    # Check if image exist
    if not os.path.isfile(args['image']):
        raise OSError(TAG + "File doesn't exist: %s" % args['image'])

     # Decode the image and extract type
    image, imageType = load_pil_image(args['image'])
    width, height = image.size

    

    # Recognize/Process
    # Please note that the first time you call this function all deep learning models will be loaded 
    # and initialized which means it will be slow. In your application you've to initialize the engine
    # once and do all the recognitions you need then, deinitialize it.
    checkResult("Process",
                ultimateAlprSdk.UltAlprSdkEngine_process(
                    imageType,
                    image.tobytes(), # type(x) == bytes
                    width,
                    height,
                    0, # stride
                    1 # exifOrientation (already rotated in load_image -> use default value: 1)
                )
        )
    
    