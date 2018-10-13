import numpy as np
import cv2
from getkeys import key_check
import os
import json

import time
import _thread

training_data = []

keys = key_check()

a = 0

thread_num = 0

def get_json_data():
    datos = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]
    with open("data_file.json", "r") as read_file:
        try:
            data = json.load(read_file)
            read_file.close("data_file.json")
        except:
            pass

    if (data['car']['incar'] == True):
        datos[0] = int(float(data['car']['speed']))
        #datos[1] = data['car']['incar']
        datos[1] = data['car']['coordsX']
        datos[2] = data['car']['coordsY']
        datos[3] = data['car']['coordsZ']
        datos[4] = data['car']['direction']
        datos[5] = data['car']['distance']
        datos[6] = int(float(data['car']['heading']))
        datos[7] = data['car']['on_road']
        datos[8] = data['car']['destinationX']
        datos[9] = data['car']['destinationY']
        datos[10] = data['car']['destinationZ']
        
        # print(datos)
        
        return datos
    # print("On road: ", data['car']['on_road'])
    # print("speed",data['car']['speed'])

    '''
    elif (data['car']['incar'] == False):
        print("You are not in a car, get in to one")
        return None
    '''

def main(): 
    
    debug = False
    prev = np.array([1])
    thread_num = 0
    justOne = True
    start = 0

    img = cv2.imread('test.bmp', 0)
    img2 = img
    #cv2.imshow('window', img)
    cv2.moveWindow('window', 900, 0)
    # cv2.imshow('otra', img2)
    # cv2.moveWindow('otra', 900, 600)
    cv2.waitKey(10)
    
    for i in range(3):   
        print(i + 1)
        time.sleep(1)

    last_time = time.time()
    paused = False
    
    print('STARTING!!!')
    end = 0
    while (True):
        try:
            if debug:
                json_data = get_json_data()
                print(json_data)
           
            img = cv2.imread('test.bmp', 1)
            
            start = time.time()
            if (np.array_equal(img, prev)):
                end = time.time()
                #print("loop: ", end-start)
                #print("equal")
                pass
            else:
                img2 = cv2.imread('tost.bmp', 0)
                cv2.imshow('window', img) # cv2.resize(img, (900, 700)))
                cv2.imshow('window2', img2)
                if cv2.waitKey(1) & 0xFF == ord('q'):
                    cv2.destroyAllWindows()
                    break
                    
                json_data = get_json_data()
                print(json_data)
                #horizontal = np.hstack((img2, img))
               
                #cv2.imshow('otra', img2)
                                   
                prev = img
            
        except:
            
            pass            
                                        
        keys = key_check()
        if 'T' in keys:
            if paused:
                paused = False
                print('unpaused!')
            else:
                print('Pausing!')
                paused = True
            time.sleep(1)

        elif 'R' in keys:
            print("ReStarted")
            print("Pause: ", paused)
            main()


        elif 'Y' in keys:
            if debug:
                debug = False
                print('NO DEBUG')
                time.sleep(1)
            else:
                print('DEBUG ON')
                debug = True
                time.sleep(1)


main()
