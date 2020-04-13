clc
clear

tcpipServer = tcpip('0.0.0.0',55001,'NetworkRole','Server', 'INPUT', 1000000);
set(tcpipServer,'Timeout',30);

fopen(tcpipServer);

try
    while(1)
        imageDataLength = fread(tcpipServer, 1, 'uint32');
        fprintf("img length: %d\n", imageDataLength);
        imageData = uint8(fread(tcpipServer, imageDataLength, 'uint8'));
        
        imageData = reshape(imageData, 3, []);
        imageData = imageData.';
        imageData = reshape(imageData, 640, 480, []);
        imageData = imrotate(imageData, 90);
        
%         fid = fopen("picture/p-"+ datestr(now,'dd-mmm-yyyy HH-MM-SS') + '.png', 'w');
%         fwrite(fid, imageData, 'uint8');
%         fclose(fid);
        
%         imshow(imageData);
        imwrite(imageData, ['picture/p-',datestr(now,'dd-mmm-yyyy HH-MM-SS'),'.png']);
    end
catch e
    disp(e);
    disp(e.identifier);
    disp(e.message);
    disp(e.stack);
    disp(e.cause);
end

fclose(tcpipServer);