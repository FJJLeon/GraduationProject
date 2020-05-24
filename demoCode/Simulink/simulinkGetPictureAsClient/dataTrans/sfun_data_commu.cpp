/* Copyright 2003-2014 The MathWorks, Inc. */

// *******************************************************************
// **** To build this mex function use: mex sfun_cppcount_cpp.cpp ****
// *******************************************************************

#include "sfun_data_commu.h"

#define S_FUNCTION_LEVEL 2
#define S_FUNCTION_NAME  sfun_data_commu

// Need to include simstruc.h for the definition of the SimStruct and
// its associated macro definitions.
#include "simstruc.h"

//#include <windows.h>
#include <string>
#include <winsock2.h>
#include <ws2tcpip.h> // for addrinfo

#pragma comment(lib,"ws2_32.lib")

#define IS_PARAM_DOUBLE(pVal) (mxIsNumeric(pVal) && !mxIsLogical(pVal) &&\
!mxIsEmpty(pVal) && !mxIsSparse(pVal) && !mxIsComplex(pVal) && mxIsDouble(pVal))

// The S-function has following parameters
// 
//  server address
//  server port
//  sample time/s
enum {
    serverAddrIdx = 0,
    serverPortIdx,
    sampleTimeIdx,
    PRMCount
};
#define PRM_ServerAddr(S) (std::string(mxArrayToString(ssGetSFcnParam(S, serverAddrIdx))));
#define PRM_ServerPort(S) ((int)mxGetScalar(ssGetSFcnParam(S, serverPortIdx)));
#define PRM_SampleTime(S) ((int)mxGetScalar(ssGetSFcnParam(S, sampleTimeIdx)));



// Function: mdlInitializeSizes ===============================================
// Abstract:
//    The sizes information is used by Simulink to determine the S-function
//    block's characteristics (number of inputs, outputs, states, etc.).
static void mdlInitializeSizes(SimStruct *S)
{
    // No expected parameters
    ssSetNumSFcnParams(S, PRMCount);

    // Parameter mismatch will be reported by Simulink
    if (ssGetNumSFcnParams(S) != ssGetSFcnParamsCount(S)) {
        return;
    }

    // Specify I/O
    if (!ssSetNumInputPorts(S, 2)) return;
    ssSetInputPortWidth(S, 0, DYNAMICALLY_SIZED);
    ssSetInputPortDirectFeedThrough(S, 0, 1);
    if (!ssSetNumOutputPorts(S,1)) return;
    ssSetOutputPortWidth(S, 0, DYNAMICALLY_SIZED);

    ssSetNumSampleTimes(S, 1);

    // Reserve place for C++ object
    ssSetNumPWork(S, 1);

    ssSetOperatingPointCompliance(S, USE_CUSTOM_OPERATING_POINT);

    ssSetOptions(S,
                 SS_OPTION_WORKS_WITH_CODE_REUSE |
                 SS_OPTION_EXCEPTION_FREE_CODE |
                 SS_OPTION_DISALLOW_CONSTANT_SAMPLE_TIME);

}


// Function: mdlInitializeSampleTimes =========================================
// Abstract:
//   This function is used to specify the sample time(s) for your
//   S-function. You must register the same number of sample times as
//   specified in ssSetNumSampleTimes.
static void mdlInitializeSampleTimes(SimStruct *S)
{
    ssSetSampleTime(S, 0, INHERITED_SAMPLE_TIME);
    ssSetOffsetTime(S, 0, 0.0);
    ssSetModelReferenceSampleTimeDefaultInheritance(S); 
}

// Function: mdlStart =======================================================
// Abstract:
//   This function is called once at start of model execution. If you
//   have states that should be initialized once, this is the place
//   to do it.
#define MDL_START
static void mdlStart(SimStruct *S)
{   
    int iResult;
    // winsock initialize
    WSADATA wsadata;
    iResult = WSAStartup(MAKEWORD(2, 2), &wsadata);
    if (iResult != 0) {
        ssPrintf("WSAStartup failed: %d\n", iResult);
        return;
    }
    // check version
    if (LOBYTE(wsadata.wVersion) != 2 || HIBYTE(wsadata.wHighVersion) != 2) {
        ssPrintf("socket version not match!!\n");
		WSACleanup();
    }
    // store SOCKET object in pointer work vector
    ssGetPWork(S)[0] = (void *) new SOCKET;
    SOCKET* pSock = (SOCKET *)ssGetPWork(S)[0];
    *pSock = INVALID_SOCKET;
    // declare addrinfo that contains a sockaddr structure
    struct addrinfo *result = NULL,
                    *ptr = NULL,
                    hints;
    ZeroMemory(&hints, sizeof(hints));
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_protocol = IPPROTO_TCP;
    // Resolve the server address and port
    std::string serverAddr = PRM_ServerAddr(S);
    int serverPort = PRM_ServerPort(S);
    ssPrintf("get remote server info: %s:%d\n", serverAddr.c_str(), serverPort);
    // getaddrinfo
    char charPort[5];
    sprintf(charPort, "%d", serverPort);
    iResult = getaddrinfo(serverAddr.c_str(), charPort, &hints, &result);
    if (iResult != 0) {
        ssPrintf("getaddrinfo failed: %d\n", iResult);
        WSACleanup();
        return;
    }
    // create a SOCKET for connecting to server
    ptr = result;
    *pSock = socket(ptr->ai_family, ptr->ai_socktype, ptr->ai_protocol);
    if (*pSock == INVALID_SOCKET) {
        ssPrintf("Error at socket(): %ld\n", WSAGetLastError());
        freeaddrinfo(result);
        WSACleanup();
        return;
    }
    // Set TimeOut
    int recvTimeout = 1 * 1000;  //1s
    int sendTimeout = 1 * 1000;  //1s

    setsockopt(*pSock, SOL_SOCKET, SO_RCVTIMEO, (char *)&recvTimeout, sizeof(int));
    setsockopt(*pSock, SOL_SOCKET, SO_SNDTIMEO, (char *)&sendTimeout, sizeof(int));

    // connect to server.
    iResult = connect(*pSock, ptr->ai_addr, (int)ptr->ai_addrlen);
    if (iResult == SOCKET_ERROR) {
        closesocket(*pSock);
        *pSock = INVALID_SOCKET;
    }
    freeaddrinfo(result);
    if (*pSock == INVALID_SOCKET) {
        ssPrintf("Unable to connect to server!\n");
        WSACleanup();
        return;
    }
    
    // once connect, receive init posture info
    struct MyPosture initPosture;
    int size = sizeof(MyPosture);
    
    memset((char*)&initPosture, 0, size);
    iResult = recv(*pSock, (char*)&initPosture, size, 0);
    if (iResult > 0) {
        ssPrintf("bytes received: %d, positon x=%f, y=%f, z=%f\n",
                iResult, initPosture.myPosition.x, initPosture.myPosition.y, initPosture.myPosition.z);
    }
    else if (iResult == 0)
        ssPrintf("Connection closed\n");
    else
        ssPrintf("Receive failed: %d\n", WSAGetLastError());
}

// Function: mdlOutputs =======================================================
// Abstract:
//   In this function, you compute the outputs of your S-function
//   block.
static void mdlOutputs(SimStruct *S, int_T tid)
{
    // Retrieve TCP Socket from the work pointers vector
    SOCKET *pSock = static_cast<SOCKET *>(ssGetPWork(S)[0]);
/* 
    // Get data addresses of I/O
//     InputRealPtrsType  u = ssGetInputPortRealSignalPtrs(S,0);
//                real_T *y = ssGetOutputPortRealSignal(S, 0);

    // Call AddTo method and return peak value
    y[0] = da->AddTo(*u[0]);
*/
    // loop for data switch
//     int iResult;
//     char send_buf[5] = {'a', 'b', 'c', 'd', 'e'};
//     int send_len = send(*pSock, send_buf, 5, 0);
//     if (send_len < 0) {
//         ssPrintf("send message %s failed£¡\n", send_buf);
//     }
//     
//     struct cube {
//         int length;
//         int width;
//         double height;
//     } c;
//     struct dirAndSpeed {
//         int dir;
//         int speed;
//     } ds;
//     
//     ssPrintf("size cube: %d, dirAndSpeed: %d\n", sizeof(cube), sizeof(dirAndSpeed));
//     
//     ssPrintf("mdlOutputs: Simulate now begin recv\n");
//     char chartag;
//     char charlen;
//     struct cube cube_buf;
//     struct dirAndSpeed ds_buf;
//     
//     iResult = recv(*pSock, (char*)&chartag, 1, 0);
//     int tag =  (int)chartag;
//     ssPrintf("mdlOutputs: Simulate recv tag: %d\n", tag);
//     iResult = recv(*pSock, (char*)&charlen, 1, 0);
//     int len = (int)charlen;
//     ssPrintf("mdlOutputs: Simulate recv len: %d\n", len);
//     switch (tag) {
//         case 1:
//             ssPrintf("switch tag 1\n");
//             memset((char*)&cube_buf, 0, sizeof(cube));
//             iResult = recv(*pSock, (char*)&cube_buf, sizeof(cube), 0);
//             //assert(len == sizeof(cube))
//             ssPrintf("cube received, len:%d, width:%d, height:%f\n", cube_buf.length, cube_buf.width, cube_buf.height);
//             break;
//         case 2:
//             ssPrintf("switch tag 2\n");
//             memset((char*)&ds_buf, 0, sizeof(dirAndSpeed));
//             iResult = recv(*pSock, (char*)&ds_buf, sizeof(dirAndSpeed), 0);
//             ssPrintf("dirAndSpeed received, dir:%d, speed:%d\n", ds_buf.dir, ds_buf.speed);
//             break;
//         default:
//             ssPrintf("unknown tag");
//     }
}

#ifdef MATLAB_MEX_FILE
/* Define to indicate that this S-Function has the mdlG[S]etOperatingPoint methods */
#define MDL_OPERATING_POINT

/* Function: mdlGetOperatingPoint ==================================================
 * Abstract:
 *    Save the operating point of this block and return it to Simulink 
 */
static mxArray* mdlGetOperatingPoint(SimStruct* S)
{  
//     DoubleAdder *da = static_cast<DoubleAdder*>(ssGetPWork(S)[0]);
//     return mxCreateDoubleScalar(da->GetPeak());
    return NULL;
}
/* Function: mdlSetOperatingPoint =================================================
 * Abstract:
 *   Restore the operating point of this block based on the provided data (ma)
 *   The data was saved by mdlGetOperatingPoint
 */
static void mdlSetOperatingPoint(SimStruct* S, const mxArray* ma)
{
    // Retrieve C++ object from the pointers vector
//     DoubleAdder *da = static_cast<DoubleAdder*>(ssGetPWork(S)[0]);
//     da->SetPeak(mxGetPr(ma)[0]);
}
#endif // MATLAB_MEX_FILE

// Function: mdlTerminate =====================================================
// Abstract:
//   In this function, you should perform any actions that are necessary
//   at the termination of a simulation.  For example, if memory was
//   allocated in mdlStart, this is the place to free it.
static void mdlTerminate(SimStruct *S)
{
    // Retrieve and destroy C++ object
    SOCKET *pSock = static_cast<SOCKET *>(ssGetPWork(S)[0]);
    const char *simStop = "Simulate Terminated";
    int iResult = send(*pSock, simStop, (int)strlen(simStop), 0);
    if (iResult == SOCKET_ERROR) {
        ssPrintf("mdlTerminate, send fail: %d\n", WSAGetLastError());
    }
    WSACleanup();
    delete pSock;
}


// Required S-function trailer
#ifdef  MATLAB_MEX_FILE    /* Is this file being compiled as a MEX-file? */
#include "simulink.c"      /* MEX-file interface mechanism */
#else
#include "cg_sfun.h"       /* Code generation registration function */
#endif
