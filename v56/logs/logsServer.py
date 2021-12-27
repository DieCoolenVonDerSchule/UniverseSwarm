import socket
import threading
import pickle



HEADER = 2048
PORT = 5080
SERVER = "0.0.0.0"
ADDR = (SERVER, PORT)
FORMAT = 'utf-8'
DISCONNECT_MESSAGE = "!DISCONNECT"


print("SERVER:",SERVER)
print("PORT:",PORT)


server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server.bind(ADDR)


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

def handle_client(conn, addr):
    print(f"[NEW CONNECTION] {addr} connected.")

    hname = receive(conn)
    nname = receive(conn)
    nname = nname[:-1]
    sname = receive(conn)
    lname = receive(conn)

    with open(f"{nname}-{sname}-{lname}-{hname}.txt", "a") as logfile:
        logfile.write(receive(conn)+"\n")

    
    conn.close()

def start():
    server.listen()
    print(f"[LISTENING] Server is listening on {SERVER}")
    while True:
        thread = threading.Thread(target=handle_client, args=(server.accept()))
        thread.start()
        print(f"[ACTIVE CONNECTIONS] {threading.activeCount() - 1}")

start()
