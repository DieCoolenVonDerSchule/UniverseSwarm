#!/usr/bin/env python3

import socket
import threading
import json

import os
import struct
import numpy as np

import time

import math
from perlin_noise import PerlinNoise
from datetime import datetime
from signal import signal, SIGPIPE, SIG_DFL
from noise import snoise3
signal(SIGPIPE,SIG_DFL) 


X = 50
Y = 50
Z = 50

STARTX = int(np.random.rand()*1000)
STARTY = int(np.random.rand()*1000)
STARTZ = int(np.random.rand()*1000)
SCALE = 0.01

print("STARTX:",STARTX)
print("STARTY:",STARTY)
print("STARTZ:",STARTZ)



HEADER = 2048
PORT = 5052
#SERVER = socket.gethostbyname(socket.gethostname())
SERVER = '0.0.0.0'
ADDR = (SERVER, PORT)
FORMAT = 'utf-8'
CREATE_MESSAGE = "!CREATE"
CONFIRMATION_MESSAGE = "[WORKER SAYS] thank you"

waitingThreads = []


print("SERVER:",SERVER)
print("PORT:",PORT)


server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server.bind(ADDR)

 
    

def create_densitymap(x, y, z, mapsize, start, scale, seed):
    print("[CREATING] densitymap..")
    
    #MESSUNG START
    now = datetime.now()
    timestamp = datetime.timestamp(now)
    print("[TIME] start: ", timestamp)

    densitymap = np.zeros([x, y, z])
    
    center = np.array([mapsize/2, mapsize/2, mapsize/2])
    maxDistance = np.linalg.norm(center - np.array([0,0,0]))
       
    print("CENTER:",center)
    print("MAXDISTANCE:",maxDistance)

    shape = (int(x), int(y), int(z))
    res = (int(x/16), int(y/16), int(z/16))
    noise = generate_fractal_noise_3d(shape, seed, start, scale, octaves=5, persistence=0.5)
    
    for i in range(0, x):
        for j in range(0, y):
            for k in range(0, z):
                densitymap[i][j][k] = (1 - (np.linalg.norm(np.array([start[0]+i,start[1]+j,start[2]+k]) - center) / maxDistance)) + noise[i,j,k] * 0.2
                if densitymap[i][j][k] == 0:
                    print(f"[WARNING] i: {i}, j: {j}, k: {k} computes to 0!")

    
    #MESSUNG STOP
    now2 = datetime.now()
    timestamp2 = datetime.timestamp(now2)
    print("[TIME] stop: ", timestamp2)


    #DURATION
    diff = timestamp2 - timestamp
    print("[TIME] duration: ", diff)

    sendlogs("create", diff)
    
    
    print("CONTENT TYPE:", type(densitymap[0][0][0]))
    print("..done")

    planet = json.dumps(densitymap.tolist())
    return planet
    
    
    
def generate_perlin_noise_3d(shape, res, seed, start):
    def f(t):
        return 6*t**5 - 15*t**4 + 10*t**3

    np.random.seed(seed)
    delta = (res[0] / shape[0], res[1] / shape[1], res[2] / shape[2])
    d = (shape[0] // res[0], shape[1] // res[1], shape[2] // res[2])
    grid = np.mgrid[0:res[0]:delta[0],0:res[1]:delta[1],0:res[2]:delta[2]]
    grid = grid.transpose(1, 2, 3, 0) % 1
    # Gradients
    theta = 2*np.pi*np.random.rand(res[0]+1, res[1]+1, res[2]+1)
    phi = 2*np.pi*np.random.rand(res[0]+1, res[1]+1, res[2]+1)
    print(f"PHI: {phi}")
    print(f"THETA: {theta}")

    gradients = np.stack((np.sin(phi)*np.cos(theta), np.sin(phi)*np.sin(theta), np.cos(phi)), axis=3)
    gradients[-1] = gradients[0]
    g000 = gradients[0:-1,0:-1,0:-1].repeat(d[0], 0).repeat(d[1], 1).repeat(d[2], 2)
    g100 = gradients[1:  ,0:-1,0:-1].repeat(d[0], 0).repeat(d[1], 1).repeat(d[2], 2)
    g010 = gradients[0:-1,1:  ,0:-1].repeat(d[0], 0).repeat(d[1], 1).repeat(d[2], 2)
    g110 = gradients[1:  ,1:  ,0:-1].repeat(d[0], 0).repeat(d[1], 1).repeat(d[2], 2)
    g001 = gradients[0:-1,0:-1,1:  ].repeat(d[0], 0).repeat(d[1], 1).repeat(d[2], 2)
    g101 = gradients[1:  ,0:-1,1:  ].repeat(d[0], 0).repeat(d[1], 1).repeat(d[2], 2)
    g011 = gradients[0:-1,1:  ,1:  ].repeat(d[0], 0).repeat(d[1], 1).repeat(d[2], 2)
    g111 = gradients[1:  ,1:  ,1:  ].repeat(d[0], 0).repeat(d[1], 1).repeat(d[2], 2)
    # Ramps
    n000 = np.sum(np.stack((grid[:,:,:,0]  , grid[:,:,:,1]  , grid[:,:,:,2]  ), axis=3) * g000, 3)
    n100 = np.sum(np.stack((grid[:,:,:,0]-1, grid[:,:,:,1]  , grid[:,:,:,2]  ), axis=3) * g100, 3)
    n010 = np.sum(np.stack((grid[:,:,:,0]  , grid[:,:,:,1]-1, grid[:,:,:,2]  ), axis=3) * g010, 3)
    n110 = np.sum(np.stack((grid[:,:,:,0]-1, grid[:,:,:,1]-1, grid[:,:,:,2]  ), axis=3) * g110, 3)
    n001 = np.sum(np.stack((grid[:,:,:,0]  , grid[:,:,:,1]  , grid[:,:,:,2]-1), axis=3) * g001, 3)
    n101 = np.sum(np.stack((grid[:,:,:,0]-1, grid[:,:,:,1]  , grid[:,:,:,2]-1), axis=3) * g101, 3)
    n011 = np.sum(np.stack((grid[:,:,:,0]  , grid[:,:,:,1]-1, grid[:,:,:,2]-1), axis=3) * g011, 3)
    n111 = np.sum(np.stack((grid[:,:,:,0]-1, grid[:,:,:,1]-1, grid[:,:,:,2]-1), axis=3) * g111, 3)
    # Interpolation
    t = f(grid)
    n00 = n000*(1-t[:,:,:,0]) + t[:,:,:,0]*n100
    n10 = n010*(1-t[:,:,:,0]) + t[:,:,:,0]*n110
    n01 = n001*(1-t[:,:,:,0]) + t[:,:,:,0]*n101
    n11 = n011*(1-t[:,:,:,0]) + t[:,:,:,0]*n111
    n0 = (1-t[:,:,:,1])*n00 + t[:,:,:,1]*n10
    n1 = (1-t[:,:,:,1])*n01 + t[:,:,:,1]*n11
    return ((1-t[:,:,:,2])*n0 + t[:,:,:,2]*n1)

   


def generate_fractal_noise_3d(shape, seed, start, scale, octaves=1, persistence=0.5):
    
    map = np.zeros(shape)

    print(start)

    for i in range(0, shape[0]):
        for j in range(0, shape[1]):
            for k in range(0, shape[2]):
                map[i][j][k] = snoise3((i+seed+start[0])*scale, (j+start[1])*scale, (k+start[2])*scale, octaves=octaves, persistence=persistence)

    return map




def send(addr, msg):
    #SEND MSG
    message = msg.encode(FORMAT)
    msg_length = len(message)
    send_length = str(msg_length).encode(FORMAT)
    send_length += b' ' * (HEADER - len(send_length))
    print(f"[SENDING] {msg_length} bytes")
    addr.sendall(send_length)
    print(f"[SENDING] densitymap segment to hive")
    addr.sendall(message)
    print("..done")
    

def receive(addr):
    #RECEIVE MSG
    print(f"[RECEIVING] {HEADER} bytes")
    msg_length = addr.recv(HEADER, socket.MSG_WAITALL).decode(FORMAT)
    if msg_length:
        msg_length = int(msg_length)
        print(f"[RECEIVING] data...")
        msg = addr.recv(msg_length, socket.MSG_WAITALL).decode(FORMAT)
        print(f"[RECEIVED] {msg_length} bytes")
        return msg
        
        
def sendBlockInfo(client, posx,posy,posz,edgeLength):
    print(f"[SENDING] blockinfo x={posx}, y={posy}, z={posz}, edge={edgeLength} to hive")
    send(client, f"{posx}#{posy}#{posz}#{edgeLength}")
    
    
        

def handle_client(conn, addr):
    print(f"[NEW CONNECTION] {addr} connected.")
    
    #RECEIVE BLOCKINFO FROM HIVE
    block= receive(conn).split("#")
    posx = int(block[0])
    posy = int(block[1])
    posz = int(block[2])
    edgeLength = int(block[3])
    mapsize = int(block[4])
    seed = int(block[5])

    
    print(f"[RECEIVED] blockinfo x={posx}, y={posy}, z={posz}, edge={edgeLength}, mapsize={mapsize}, seed={seed} from hive")
    
    #CREATE DENSITYMAP
    densitymap = create_densitymap(edgeLength, edgeLength, edgeLength, mapsize, (posx, posy, posz), SCALE, seed)
    
    #SEND BLOCKINFO TO HIVE
    sendBlockInfo(conn, posx, posy, posz, edgeLength)
    
    #SEND DENSITYMAP SEGMENT TO HIVE
    #MESSUNG START
    now = datetime.now()
    timestamp = datetime.timestamp(now)
    print("[TIME] start: ", timestamp)

    send(conn, densitymap)
    
    #MESSUNG STOP
    now2 = datetime.now()
    timestamp2 = datetime.timestamp(now2)
    print("[TIME] stop: ", timestamp2)


    #DURATION
    diff = timestamp2 - timestamp
    print("[TIME] duration: ", diff)

    sendlogs("send", diff)
    
    print(f"Sending to {addr}")

    print(receive(conn))
    
    conn.close()
    print(f"[CONNECTION CLOSED] {addr}")
        
def sendlogs(lname, text):
    client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    client.connect(("lazyloki.ddns.net", 5080))

    stream = os.popen("/sbin/ip route|awk '/default/ { print $3 }'")
    output = stream.read()

    send(client, socket.gethostname())
    send(client, output)
    send(client, "WORKER")
    send(client, lname)
    send(client, str(text))

    client.close()

def handleWaitList():
    while True:
        if threading.activeCount() - 2 <= 0 and len(waitingThreads) > 0:
            print(f"[ACTIVE CONNECTIONS] {threading.activeCount() - 1}")
            waitingThreads.pop(0).start()
            #thread.start()
            
     
        time.sleep(2)

def start():
    server.listen()
    waitListThread = threading.Thread(target=handleWaitList, args=())
    waitListThread.start()
    print(f"[LISTENING] Server is listening on {SERVER}")
    while True:
        conn, addr = server.accept()
        print("[CONNECTION RECEIVED]")
        thread = threading.Thread(target=handle_client, args=(conn, addr))
        waitingThreads.append(thread)
        
    
    

print("[STARTING] server is starting...")
start()





