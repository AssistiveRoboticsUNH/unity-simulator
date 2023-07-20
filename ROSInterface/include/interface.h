#pragma once

#include "Eigen/Core"
#include <optional>

#if defined(_MSC_VER)
#define MY_LIB_API __declspec(dllexport) // Microsoft
#elif defined(__GNUC__)
#define MY_LIB_API __attribute__((visibility("default"))) // GCC
#else
#define MY_LIB_API // Most compilers export all the symbols by default. We hope for the best here.
#pragma warning Unknown dynamic link import/export semantics.
#endif

struct Quaternion {
    double x = 0;
    double y = 0;
    double z = 0;
    double w = 1;

};

struct NativeVector3 {
    double x = 0;
    double y = 0;
    double z = 0;

};

struct NativeInt32 {
    char *topic;
    int data;
};


struct NativeTwist {
    NativeVector3 linear;
    NativeVector3 angular;

};

struct NativeTransform {
    char *frame_id = nullptr;
    char *child_frame_id = nullptr;
    NativeVector3 translation;
    Quaternion rotation;
};

struct NativeOdom {
    NativeTransform pose;
    double pose_covariance[36] = {};
    NativeVector3 linear_velocity;
    NativeVector3 angular_velocity;
    double twist_covariance[36] = {};
};

struct NativeImage {
    char *topic;
    int height;
    int width;
    int step;
    char *frame_id;
    char *encoding;
    uint8_t *data;
};

struct NativeScan {
    char *topic;
    char *frame_id;
    float angle_min;
    float angle_max;
    float angle_increment;
    float time_increment;
    float scan_time;
    float range_min;
    float range_max;
    int count;
    float *ranges;
    float *intensities;
};


void initROS();

extern "C" {
MY_LIB_API void PublishTF(std::intptr_t handle, NativeTransform *input);
MY_LIB_API void PublishInt32(std::intptr_t handle, NativeInt32 *input);
MY_LIB_API void PublishOdom(std::intptr_t handle, NativeOdom *input);
MY_LIB_API void PublishImage(std::intptr_t handle, NativeImage *input);
MY_LIB_API void PublishScan(std::intptr_t handle, NativeScan *input);
MY_LIB_API void ReceiveCmdVel(std::intptr_t handle, NativeTwist *output);
MY_LIB_API std::intptr_t Init();
MY_LIB_API void Destroy(std::intptr_t handle);
}
