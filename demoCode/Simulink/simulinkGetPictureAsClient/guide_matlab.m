% clear;
guideClient = tcpip('127.0.0.1',56000,'NetworkRole','client');
set(guideClient, 'InputBufferSize', 3000000);
set(guideClient, 'Timeout', 30);

fopen(guideClient);

fwrite(guideClient, 'Matlab')

pause(2);

recv_1 = fread(guideClient, 5, 'char');
check = sprintf("%s", recv_1);
fprintf('recv from Guide: [%s] with len=%d\n', check, strlength(check));
if "CHECK" == check
    disp("CEHCK cmd get");
else
    disp("CHECK cmd wrong");
    return;
end

block = find_system('tcpRecv_view', 'BlockType', 'Reshape');
param = get_param(block(2), 'OutputDimensions');
from_model = param{1};
resolv = sprintf('Matlab:%s', from_model);
send_1 = sprintf("%30s", resolv);
fwrite(guideClient, send_1)

recv_2 = fread(guideClient, 5, 'char');
start = sprintf("%s", recv_2);
fprintf('recv from Guide: [%s] with len=%d\n', start, strlength(start));
if "START" == start
    disp("START cmd get");
else
    disp("START cmd wrong");
    return;
end

response_ok = sprintf("Matlab:[OK]");
send_2 = sprintf("%30s", response_ok);
fwrite(guideClient, send_2)

sim("tcpRecv_view");


