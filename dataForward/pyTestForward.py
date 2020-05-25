import socket
import threading

def sendAndRecv(sock, i):
    recv_list = []
    sock.settimeout(3)
    for c in range(20):
        text = " 来自 {0} 的数据 {1} ".format(i, c)
        print("直接打印{0}".format(text))
        sock.sendall(text.encode("utf-8"))
        print("{0} 发送成功".format(i))
        recv_data = sock.recv(1024).decode("utf-8")
        print("{0} 接收成功".format(i))
        if recv_data:
            # recv_list.append(recv_data)
            print("{0} 接收到数据： {1}".format(i, recv_data))
    print("{0} 已经退出了".format(i))
#     if i != 2:
#         return;
#     print("{0} 发送完毕，以下为接收数据：".format(i))
#     for rd in recv_list:
#         print(rd);
            

def main():

    for i in range(5):
        # Create a TCP/IP socket
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

        # Connect the socket to the port where the server is listening
        server_address = ('localhost', 55555)
        sock.connect(server_address)

        t = threading.Thread(target=sendAndRecv, args=(sock, i))
        t.start()

if __name__ == "__main__":
    main()


    
