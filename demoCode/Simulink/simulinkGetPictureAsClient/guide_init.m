% clear;
tcpipServer = tcpip('127.0.0.1',56000,'NetworkRole','client');
set(tcpipServer, 'InputBufferSize', 3000000);
set(tcpipServer, 'Timeout', 30);

fopen(tcpipServer);

fwrite(tcpipServer, 'Matlab')
% sim("tcpRecv_view");