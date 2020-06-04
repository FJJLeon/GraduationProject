/* Copyright 2003-2009 The MathWorks, Inc. */

#ifndef _SFUN_DATA_COMMU_
#define _SFUN_DATA_COMMU_

// Define a struct for posture
// position
struct MyPosition
{
    float x;
    float y;
    float z;
};
// quaternion
struct MyQuaternion
{
    float x;
    float y;
    float z;
    float w;
};
struct MyPosture
{
    struct MyPosition myPosition;
    struct MyQuaternion myQuaternion;
};
// Define for euler angle
struct MyEulerAngles
{
    float pitch;  // X
    float yaw;    // Y
    float roll;   // Z  
};

// Define a struct for move control
struct MoveCTL
{
    float vertical;
    float horizonal;
};


#endif


