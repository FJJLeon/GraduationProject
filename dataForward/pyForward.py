import socket
import threading
import queue
import sys

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
                        to_sock.send(data)
                except:
                    self.client_list.remove(to_sock)

    
def main():
    msg_queue = queue.Queue()
    client_list = []

    tcp_server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_addr = ('', 55555)
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

    
    
if __name__ == "__main__":
    main()




    
