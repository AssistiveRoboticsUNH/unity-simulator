#include "interface.h"

int main() {
    NativeOdom input;
    std::intptr_t handle = NULL;
    PublishOdom(handle, &input);
    return 0;
}