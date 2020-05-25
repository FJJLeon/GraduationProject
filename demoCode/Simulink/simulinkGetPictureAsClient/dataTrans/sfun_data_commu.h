/* Copyright 2003-2009 The MathWorks, Inc. */

#ifndef _SFUN_DATA_COMMU_
#define _SFUN_DATA_COMMU_

// Define a struct for posture
struct MyPosture
{
    struct MyPosition
    {
        float x;
        float y;
        float z;
    };
    MyPosition myPosition;
    struct MyQuaternion
    {
        float x;
        float y;
        float z;
        float w;
    };
    MyQuaternion myQuaternion;
};

// Define a struct for command
enum CMDTYPE 
{
    CMDTYPE_MOVE = 5000,
    TYPE
};

struct MoveCMD
{
    float vertical;
    float horizonal;
};
struct Command 
{
    CMDTYPE type;
    int len;
    MoveCMD movecmd;
};

#endif


