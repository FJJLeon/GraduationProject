matfile = load("testimg523.mat");
datasize = size(matfile.panor.Data);
disp(datasize);

count = datasize(4);
for i = 1:count
    imshow(matfile.panor.Data(:,:,:,i));
    pause(0.5);
end