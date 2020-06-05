import socket
import time

def guide_center(host, port):
    s = socket.socket(socket.AF_INET,socket.SOCK_STREAM)
    s.connect((host, port)) 
    s.sendall(b'_Guide')

    s.sendall(b'CHECK')

    recv_11 = s.recv(30)
    recv_12 = s.recv(30)
    resolv_a, resolv_b = recv_11.decode('utf-8'), recv_12.decode('utf-8')
    print(resolv_a)
    print(resolv_b)

    if (resolv_a[-14:] != resolv_b[-14:]):
        print("Unity and Simulink figure resolve mismatch, fail")
        return
    
    input('按回车键开始仿真...')

    s.sendall(b'START')

    recv_21 = s.recv(30)
    recv_22 = s.recv(30)
    ok_a, ok_b = recv_21.decode('utf-8'), recv_22.decode('utf-8')
    print(ok_a)
    print(ok_b)
    if (ok_a[-4:] != "[OK]" or ok_b[-4:] != "[OK]"):
        print("Unity or Simulink start fail")

    print("仿真开始")

if __name__ == "__main__":
    guide_center('127.0.0.1', 56000)
