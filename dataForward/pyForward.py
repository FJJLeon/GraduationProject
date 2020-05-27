import socket
import threading
import queue
import sys
from enum import Enum

def recvall(sock, recv_size):
    recv_buffer = []
    while True:
        data = sock.recv(recv_size)
        if not data:
            break
        recv_buffer.extend(data)
        if len(recv_buffer) == recv_size:
            break
    return ','.join(map(str, recv_buffer))

class ForwardReceiver(threading.Thread):
    def __init__(self, client_list, msg_queue, client_sock, client_addr):
        super().__init__()
        self.client_list = client_list
        self.msg_queue = msg_queue
        self.client_sock = client_sock
        self.client_addr = client_addr

        self.client_list.append(self.client_sock)
        
    def run(self):
        # just receive, no init info
        try:
            while True:
                recv_data = self.client_sock.recv(1024)
                # recv_data = recvall(self.client_sock, 1024)
                if recv_data:
                    self.msg_queue.put((recv_data, self.client_sock))
        except socket.error:
            self.client_sock.close()
            

    
class ForwardSender(threading.Thread):
    def __init__(self, client_list, msg_queue):
        super().__init__()
        self.client_list = client_list
        self.msg_queue = msg_queue
        
    def run(self):

        while True:
            if len(self.client_list) <= 1:
                continue
            (data, from_sock) = self.msg_queue.get()
            for to_sock in self.client_list:
                try:
                    if (from_sock != to_sock):
                        to_sock.sendall(data)
                except:
                    self.client_list.remove(to_sock)

    
def forward():
    msg_queue = queue.Queue()
    client_list = []

    tcp_server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_addr = ('127.0.0.1', 55555)
    tcp_server.bind(server_addr)

    try:
        tcp_server.listen(8)
    except socket.error:
        print("fail to listen on port %s" % e)
        sys.exit(1)

    fs = ForwardSender(client_list, msg_queue)
    fs.setDaemon(True)
    fs.start()
    
    while True:
        client_sock, client_addr = tcp_server.accept()
        print("地址{0}已连接".format(client_addr))
        fr = ForwardReceiver(client_list, msg_queue, client_sock, client_addr)
        fr.setDaemon(True)
        fr.start()


Roles = Enum('Role', ('Guide', 'Matlab', 'Unity'))
RoleSet = {'Guide', 'Matlab', 'Unity'}
def guideService(host, port):
    guide_clients = {}
    
    guide_server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    guide_addr = (host, port)  # ('127.0.0.1', 56000)
    guide_server.bind(guide_addr)
    try:
        guide_server.listen(3)
    except socket.error:
        print("fail to listen on port %s" % e)
        sys.exit(1)
    print("guide service start at {0}...".format(guide_addr))
    for _ in range(3):
        client_sock, client_addr = guide_server.accept()
        print("地址{0}已连接".format(client_addr))
        rolebyte = client_sock.recv(1024)
        role = rolebyte.decode('utf-8')
        print("地址{0}已连接,role: {1}".format(client_addr, role))
        if role not in RoleSet:
            print("地址{0}的连接 role mismatch")
        guide_clients[role] = (client_sock, client_addr)
    


if __name__ == "__main__":
    guideService('127.0.0.1', 56000)
    #forward()




    
