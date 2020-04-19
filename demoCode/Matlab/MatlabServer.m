% clear;clc;close all;
% 127.0.0.1
% 192.168.1.234
Listener = tcpip('0.0.0.0',55001,'NetworkRole','server');
set(Listener,'InputBufferSize', 3000000); % if no this ,buffer 512 to short
set(Listener,'Timeout',30);
% set(Listener,'ByteOrder','littleEndian')
fopen(Listener);


width = 1024;
height = 768;

one = [];
whos one;
while (true)
    bytes = get(Listener,'BytesAvailable');
    
    if bytes > 0
        fprintf("read len %d\n", bytes);
        
        data = fread(Listener, bytes);
        whos data
        
        if bytes < 100
            continue;
        end
        
        one = [one; data];
        whos one
        
        last = data(bytes-1:bytes);
        whos last;
        
        if  last(1) == 'o' && last(2) == 'k'
            
            whos one;
            fid = fopen('myXXX.png', 'w');
            fwrite(fid, one(1:length(one)-2), 'uint8');
            fclose(fid);
            
            img = imread('myXXX.png');
            image(img)
%             
%             img = imread(one(1:length(one)-2));
%             image(img)
            
            
            one = [];
%             xx = image(1:length(image)-2);
%             pixels = reshape(typecast(xx, 'uint8'), [width,height]);
%             img = cat(3, ...
%                 transpose(reshape(pixels(3,:,:), [width,height])), ...
%                 transpose(reshape(pixels(2,:,:), [width,height])), ...
%                 transpose(reshape(pixels(1,:,:), [width,height])));
%             imshow(img);

        end
        
%         
%         
%         fprintf("123: %s\n", data);
%         pixels = reshape(typecast(data, 'uint8'), [3,width,height]);
%         img = cat(3, ...
%             transpose(reshape(pixels(3,:,:), [width,height])), ...
%             transpose(reshape(pixels(2,:,:), [width,height])), ...
%             transpose(reshape(pixels(1,:,:), [width,height])));
%         imshow(img);
%          fprintf("connect ok: %s\n",data);
    end
end

get(Listener);

% % load('xyz_all.mat');
% [m,n]=size(xyz_all);
% i=1;
% while(i<=m)
%     bytes = get(Listener,'BytesAvailable');
%     if bytes > 0
%         data = fread(Listener,bytes);
%         disp(data);
%        cmd=strcat("goto"," ",num2str(xyz_all(i,1))," ",num2str(xyz_all(i,2)),...
%       " ",num2str(xyz_all(i,3)));
%         disp(cmd);
% %         cmd="goto 1000 1000 1000";
%         i=i+1;
% %         fwrite(Listener,cmd);%fwrite������
%         fprintf(Listener,'%s\n',cmd);
%        pause(0.016777)
%    end
% end
fclose(Listener);