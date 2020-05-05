/* Copyright 2003-2014 The MathWorks, Inc. */

// *******************************************************************
// **** To build this mex function use: mex sfun_cppcount_cpp.cpp ****
// *******************************************************************

#include "sfun_tcp_test.h"

#define S_FUNCTION_LEVEL 2
#define S_FUNCTION_NAME  sfunc_tcp_test

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

// Function: mdlInitializeSizes ===============================================
// Abstract:
//    The sizes information is used by Simulink to determine the S-function
//    block's characteristics (number of inputs, outputs, states, etc.).
static void mdlInitializeSizes(SimStruct *S)
{
    // No expected parameters
    ssSetNumSFcnParams(S, 3);

    // Parameter mismatch will be reported by Simulink
    if (ssGetNumSFcnParams(S) != ssGetSFcnParamsCount(S)) {
        return;
    }

    // Specify I/O
    if (!ssSetNumInputPorts(S, 1)) return;
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
    // Store new C++ object in the pointers vector
    // DoubleAdder *da  = new DoubleAdder();
    // ssGetPWork(S)[0] = da;
    
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
    ZeroMemory( &hints, sizeof(hints) );
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_protocol = IPPROTO_TCP;
    // Resolve the server address and port
    std::string serverAddr = std::string(mxArrayToString(ssGetSFcnParam(S,0)));
    ssPrintf("server address: %s\n", serverAddr);
    
    *pSock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    
    SOCKADDR_IN server_addr;
    server_addr.sin_family = AF_INET;
    server_addr.sin_addr.S_un.S_addr = inet_addr("127.0.0.1");
    server_addr.sin_port = htons(55002);
    
    if (connect(*pSock, (SOCKADDR *)&server_addr, sizeof(SOCKADDR)) == SOCKET_ERROR) {
		ssPrintf("server %s:%d connect failed!!\n", "127.0.0.1", 55002);
		WSACleanup();
	}
	else {
		ssPrintf("server %s:%d connect success.\n", "127.0.0.1", 55002);
	}
    char send_buf[100] = {0};
    std::string s = "Im ur father";
    sprintf(send_buf, "%s", s.c_str());
    int send_len = send(*pSock, send_buf, 100, 0);
    if (send_len < 0) {
        ssPrintf("send message %s failed£¡\n", s);
    }
    char recv_buf[256];
    iResult = recv(*pSock, recv_buf, 256, 0);
    if (iResult > 0)
        ssPrintf("bytes received: %d, message: %s\n", iResult, recv_buf);
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
    // Retrieve C++ object from the pointers vector
    DoubleAdder *da = static_cast<DoubleAdder *>(ssGetPWork(S)[0]);
    
    // Get data addresses of I/O
    InputRealPtrsType  u = ssGetInputPortRealSignalPtrs(S,0);
               real_T *y = ssGetOutputPortRealSignal(S, 0);

    // Call AddTo method and return peak value
    y[0] = da->AddTo(*u[0]);
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
    DoubleAdder *da = static_cast<DoubleAdder*>(ssGetPWork(S)[0]);
    return mxCreateDoubleScalar(da->GetPeak());
}
/* Function: mdlSetOperatingPoint =================================================
 * Abstract:
 *   Restore the operating point of this block based on the provided data (ma)
 *   The data was saved by mdlGetOperatingPoint
 */
static void mdlSetOperatingPoint(SimStruct* S, const mxArray* ma)
{
    // Retrieve C++ object from the pointers vector
    DoubleAdder *da = static_cast<DoubleAdder*>(ssGetPWork(S)[0]);
    da->SetPeak(mxGetPr(ma)[0]);
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
    DoubleAdder *da = static_cast<DoubleAdder *>(ssGetPWork(S)[0]);
    delete da;
}


// Required S-function trailer
#ifdef  MATLAB_MEX_FILE    /* Is this file being compiled as a MEX-file? */
#include "simulink.c"      /* MEX-file interface mechanism */
#else
#include "cg_sfun.h"       /* Code generation registration function */
#endif
