cmake_minimum_required(VERSION 3.19)
project(HelloWorld-${TW_BUILD_OS}-${TW_BUILD_ARCH}-${TW_BUILD_CONFIG} LANGUAGES CXX)

add_library(HelloWorld-${TW_BUILD_OS}-${TW_BUILD_ARCH}-${TW_BUILD_CONFIG} STATIC
    "hello.cpp"
)

target_include_directories(HelloWorld-${TW_BUILD_OS}-${TW_BUILD_ARCH}-${TW_BUILD_CONFIG}
    PRIVATE ${TWWorkspaceHome}/TallyWorld/DevSupportPlatform/TWDevSupport.Source/Includes
    PRIVATE ${TWWorkspaceHome}/TallyWorld/UniversalPlatform/TWUniversal.Source/TWUniversal/Includes
)

include(TWCommonBuild)
include(TWUtilities)
