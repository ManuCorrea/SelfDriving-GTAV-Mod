import numpy as np
import cv2
from getkeys import key_check
import os
import json
import pygame
import time
import _thread
from pathlib import Path

training_data = []

keys = key_check()

#########################  Write your saving and reading paths ####################
IMG_JSON_PATH = Path('C:\\Users\\yo\\Desktop\\SelfDriving\\joystickrecolDataTraining\\MOD_DATA\\')
IMG_FRONT_PATH = str(Path.joinpath(IMG_JSON_PATH, 'front.bmp'))
IMG_LEFT_PATH = str(Path.joinpath(IMG_JSON_PATH, 'left.bmp'))

print(IMG_FRONT_PATH)


city_path_E = 'E:/TrainingData/City/'
highway_path_E = 'E:/TrainingData/Highway/'

thread_num = 0
############    Your joystick ID   ################
JOYSTICK_ID = 1
pygame.display.init()
pygame.joystick.init()
pygame.joystick.Joystick(JOYSTICK_ID).init()
print(pygame.joystick.Joystick(JOYSTICK_ID).get_name())
print(pygame.joystick.Joystick(JOYSTICK_ID).get_numaxes())

X = np.linspace(-1.0, 1.0, num=11)  # volante
Z = np.linspace(-0.3, -1.0, num=8)

# Creates directories if they do not exist
def check_dir(path):
    if not os.path.exists(path):
        os.makedirs(path)

check_dir(city_path_E)
check_dir(highway_path_E)

def check_file_exist(path, file_name, num):
    orig = file_name
    while True:
        file_name = orig.format(num)
        if os.path.isfile(os.path.join(path, file_name)):
            print('File exists, moving along', num)
            num += 1
            print(num)
        else:
            print('File does not exist, starting fresh!', num)
            return num


def get_last_file_num(city_num, highway_num):

    file_name = 'city-{}.npy'
    city_num = check_file_exist(city_path_E, file_name, city_num)

    file_name = 'highway-{}.npy'
    highway_num = check_file_exist(highway_path_E, file_name, highway_num)

    return city_num, highway_num
    

def array_keys(array_acc_br):   # This handdle 'space' and 'S' keys
    out = np.zeros(2, dtype=bool)
    keys = key_check()

    if (' ' in keys or pygame.joystick.Joystick(JOYSTICK_ID).get_button(2) == True):
        out[1] = 1  # break
        array_acc_br = np.zeros(8, dtype=bool)
    elif ('S' in keys or pygame.joystick.Joystick(JOYSTICK_ID).get_button(1) == True):
        out[0] = 1  # back
        array_acc_br = np.zeros((8), dtype=bool)
    return out


def array_acc(acc_value):
    acc = np.zeros((8), dtype=bool)  # 0,1,2 no valen
    for i in range(8):
        if (acc_value == round(Z[i], 1)):
            acc[i] = True
    return acc


def get_json_data():        #TODO: argument file_path
    datos = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]
    with open("./MOD_DATA/data_file.json", "r") as read_file:
        try:
            data = json.load(read_file)
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
                read_file.close("./MOD_DATA/data_file.json")
        except:
            pass
        
        return datos

def save_np(starting_value, training_data, thread_nume, city_or_highway):
    print('saving')
    if city_or_highway == 0:
        file_name_npy = os.path.join(city_path_E, 'city-{}.npy'.format(starting_value))
        np.save(file_name_npy, training_data)
    else:
        file_name_npy = os.path.join(highway_path_E, 'highway-{}.npy'.format(starting_value))
        np.save(file_name_npy, training_data)
    print("Thread done")


def main():
    alt_saving = False
    is_city = False
    is_highway = False
    starting_value_highway = 1
    starting_value_city = 1
    starting_value_city, starting_value_highway = get_last_file_num(starting_value_city, starting_value_highway)

    print('start val city:', starting_value_city)
    print('start val highway:', starting_value_highway)
    
    saving = False
    debug = False
    prev = np.array([1])
    thread_num = 0

    training_data = []

    img = cv2.imread(IMG_FRONT_PATH, 0)
    img2 = img
    #cv2.imshow('window', img)
    cv2.moveWindow('window', 900, 0)
    # cv2.imshow('otra', img2)
    # cv2.moveWindow('otra', 900, 600)
    cv2.waitKey(10)

    print("O for Save, R for Pause, T Paused, R Restart \nM test joystick wheel, N accelerator, Y debug JSON\n I city/highway")
    print("check correct joystick id")
    for i in range(3):
        print(i + 1)
        time.sleep(1)

    paused = False
    
    print('STARTING!!!')
    #start_time = 0
    while (True):
        #end_time = time.time()
        #print('Tiempo', end_time - start_time)
        #start_time = time.time()
        if not paused:

            pygame.event.pump()
            x = pygame.joystick.Joystick(JOYSTICK_ID).get_axis(0)
            z = pygame.joystick.Joystick(JOYSTICK_ID).get_axis(1)

            wheel = round(x, 1)
            z = round(z, 1)

            try:
                if debug:
                    json_data = get_json_data()
                    print(json_data)
               
                img = cv2.imread(IMG_FRONT_PATH, 1)
                
                cv2.imshow('window', img)
                
                if cv2.waitKey(1) & 0xFF == ord('q'):
                    cv2.destroyAllWindows()
                    break

                if saving and not np.array_equal(img, prev):
                    if alt_saving:
                        json_data = get_json_data()
                        
                        array_acc_br = array_acc(z)

                        s_space_arr = array_keys(array_acc_br)
                        acc_br_concat = np.concatenate((array_acc_br, s_space_arr))
                        if not is_highway:
                            img2 = cv2.imread(IMG_LEFT_PATH, 0)
                            training_data.append([img, img2, 1*acc_br_concat, wheel, json_data])
                            cv2.imshow('window2', img2)
                        else:
                            training_data.append([img, 1*acc_br_concat, wheel,json_data])
                        alt_saving = False
                    else:
                        alt_saving = True
                prev = img
            except cv2.error:
                pass
            
            #print(len(training_data))
            
            if len(training_data) % 100 == 0:
                if not len(training_data) == 0:
                    print("len training data", len(training_data))
                if len(training_data) == 500:
                    training_data =  np.array(training_data)
                    #print("EMPIEZA THREAD", thread_num)
                    if is_city:
                        #_thread.start_new_thread(save_np, (starting_value_city, training_data,thread_num, 0))
                        save_np(starting_value_city, training_data,thread_num, 0)
                        starting_value_city += 1
                    elif is_highway:
                        #_thread.start_new_thread(save_np, (starting_value_highway, training_data,thread_num, 1))
                        save_np(starting_value_highway, training_data,thread_num, 1)
                        starting_value_highway += 1
           
                    thread_num += 1
                                        
                    training_data = []
                                              
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

        elif 'O' in keys:
            if saving:
                saving = False
                print('NO SAVING')
            else:
                print('SAVING')
                saving = True
            time.sleep(1)

        elif 'M' in keys:
            print("Iniciando test joystick")
            for i in range(100):
                pygame.event.pump()
                x = pygame.joystick.Joystick(JOYSTICK_ID).get_axis(0)
                wheel = round(x, 1)
                print(wheel)
                time.sleep(0.2)
            main()

        elif 'N' in keys:
            print("Iniciando test joystick")
            for i in range(100):
                pygame.event.pump()
                x = pygame.joystick.Joystick(JOYSTICK_ID).get_axis(2)
                wheel = round(x, 1)

                '''
                if z>0:
                    freno = array_freno(x)
                else:
                    array_acc_br = array_acc(wheel)
                '''
                array_acc_br = array_acc(wheel)
                print(wheel)
                s_space_arr = array_keys(array_acc_br)
                acc_br_concat = np.concatenate((array_acc_br, s_space_arr))
                print(acc_br_concat)
                time.sleep(0.2)

            main()

        elif 'Y' in keys:
            if debug:
                debug = False
                print('NO DEBUG')
            else:
                print('DEBUG ON')
                debug = True
            time.sleep(1)
        elif 'I' in keys:
            if is_highway:
                is_highway = False
                is_city = True
                print('SAVING TO CITY')
            else:
                print('SAVING TO HIGHWAY')
                is_highway = True
                is_city = False
            time.sleep(1)
        '''
        end = time.time()
        '''

main()
