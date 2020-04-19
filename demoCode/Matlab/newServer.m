clc
clear

tcpipServer = tcpip('0.0.0.0',55001,'NetworkRole','Server');
set(tcpipServer, 'InputBufferSize', 3000000);
set(tcpipServer, 'Timeout', 30);

fopen(tcpipServer);

resolutionWidth = 1024;
resolutionHeight = 768;
try
    while(1)
        imageDataLength = fread(tcpipServer, 1, 'uint32');
        fprintf("img length: %d\n", imageDataLength);
        imageData = uint8(fread(tcpipServer, imageDataLength, 'uint8'));
        
        imageData = reshape(imageData, 3, []);
        imageData = imageData.';
        imageData = reshape(imageData, resolutionWidth, resolutionHeight, []);
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