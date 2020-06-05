import socket
import threading
import queue
import sys
from enum import Enum

def recvall(sock, recv_size): # no use
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
    def __init__(self, client_list, msg_queue, client_sock, client_addr, buffer_size=1024):
        super().__init__()
        self.client_list = client_list
        self.msg_queue = msg_queue
        self.client_sock = client_sock
        self.client_addr = client_addr
        self.buffer_size = buffer_size

        self.client_list.append(self.client_sock)
        
    def run(self):
        # just receive, no init info
        try:
            while True:
                recv_data = self.client_sock.recv(self.buffer_size)
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


def forward(host, port):
    msg_queue = queue.Queue()
    client_list = []

    tcp_server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_addr = (host, port)
    tcp_server.bind(server_addr)
    print("forward server {0}:{1} start listen".format(host, port))
    try:
        tcp_server.listen(2)
    except socket.error:
        print("fail to listen on port %s" % socket.error)
        sys.exit(1)

    FS = ForwardSender(client_list, msg_queue)
    # fs.setDaemon(True)
    FS.start()
    
    for _ in range(2):
        client_sock, client_addr = tcp_server.accept()
        print("client {0} connect to forward server {1}:{2} ".format(client_addr, host, port))
        FR = ForwardReceiver(client_list, msg_queue, client_sock, client_addr)
        # fr.setDaemon(True)
        FR.start()
    
    print("forward server {0}:{1} stop listen".format(host, port))

# for guide server, one server solution
Roles = Enum('Role', ('_Guide', 'Matlab', '_Unity'))
RoleSet = {'_Guide', 'Matlab', '_Unity'}

class ForwardReceiverWithRole(threading.Thread):
    def __init__(self, client_list, msg_queue, client_sock, client_addr, role, buffer_size=1024):
        super().__init__()
        self.client_list = client_list
        self.msg_queue = msg_queue
        self.client_sock = client_sock
        self.client_addr = client_addr
        self.role = role
        self.buffer_size = buffer_size
        self.debug = True

    def run(self):
        # just receive, no init info
        try:
            while True:
                recv_data = self.client_sock.recv(self.buffer_size)
                # recv_data = recvall(self.client_sock, 1024)
                if recv_data:
                    if (self.debug):
                        print("[Recever] from {0} recv: [{1}]".format(self.role, recv_data.decode('utf-8')))
                    # push (data, from_sock, from_sock_role) into queue
                    self.msg_queue.put((recv_data, self.client_sock, self.role))
        except socket.error:
            self.client_sock.close()


class ForwardSenderWithRole(threading.Thread):
    def __init__(self, client_list, msg_queue):
        super().__init__()
        self.client_list = client_list
        self.msg_queue = msg_queue
        self.debug = True
    
    def run(self):
        while True:
            # only after three client connected, start forward 
            print("guide client size: %d" % len(self.client_list))
            if len(self.client_list) != 3:
                break;
            (data, from_sock, from_role) = self.msg_queue.get()
            # modift, from guide to both, from other only to guide
            for to_sock, to_addr, to_role in self.client_list:
                # no self forward
                if (from_role == to_role):
                    continue;
                # from Guide to both         from not Guide to Guide
                if (from_role == '_Guide' or (from_role != '_Guide' and to_role == '_Guide')):
                    try:
                        if (self.debug):
                            print("[Sender] from {0} to {1}, data: [{2}]".format(from_role, to_role, data.decode('utf-8')))
                        to_sock.sendall(data)
                    except:
                        self.client_list.remove(to_sock)


def guideForward(host, port):
    msg_queue = queue.Queue()
    guide_clients = []
    
    guide_server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    guide_addr = (host, port)  # ('127.0.0.1', 56000)
    guide_server.bind(guide_addr)
    try:
        guide_server.listen(3)
    except socket.error:
        print("fail to listen on port %s" % socket.error)
        sys.exit(1)
    print("guide service start at {0}...".format(guide_addr))

    for _ in range(3):
        client_sock, client_addr = guide_server.accept()
        print("client {0} connect to guide forward server {1}:{2} ".format(client_addr, host, port))
        rolebyte = client_sock.recv(6)
        role = rolebyte.decode('utf-8')
        print("client {0} role is {1}".format(client_addr, role))
        if role not in RoleSet:
            print("client {0} role mismatch, check")
            sys.exit(1)
        guide_clients.append((client_sock, client_addr, role))
        FRwRole = ForwardReceiverWithRole(guide_clients, msg_queue, client_sock, client_addr, role, buffer_size=128);
        FRwRole.start()

    FSwRole = ForwardSenderWithRole(guide_clients, msg_queue)
    FSwRole.start()

    

    


if __name__ == "__main__":
    guideForward('127.0.0.1', 56000)

    forwardsThread = [threading.Thread(target=forward, args=('127.0.0.1', 58001)),
                      threading.Thread(target=forward, args=('127.0.0.1', 58002)),
                      threading.Thread(target=forward, args=('127.0.0.1', 58003))]
    for t in forwardsThread:
        t.start()




    
