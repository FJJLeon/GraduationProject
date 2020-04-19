clc
clear;

sender = tcpip('0.0.0.0',55002,'NetworkRole','Server');
set(sender, 'Timeout', 30);

fopen(sender);
try
    while (1)
        direct = input('输入方向（x,y,z):', 's');
        step = input('移动距离（单位:m）:');

        sendMsg = sprintf("%s %d", direct, step);
                
%         fid = fopen("picture/p-"+ datestr(now,'dd-mmm-yyyy HH-MM-SS') + '.png', 'w');
%         fwrite(fid, sendMsg, 'uint8');
%         fclose(fid);

        fwrite(sender, sendMsg);
    end
catch e
    disp(e);
    disp(e.identifier);
    disp(e.message);
    disp(e.stack);
    disp(e.cause);
end