cmake_minimum_required(VERSION 3.18)
project(Project2 LANGUAGES CXX)
include($ENV{TWWorkspaceHome}/TallyWorldPMT/TallyWorld/TWPMT.VSSetup/Wizard/Native/Properties/Common/TWCommon.cmake)

set(PROJECT_INCLUDES
    Includes
)

set(DEPENDENT_LAYERS
)

add_library(Project2 STATIC
    "HelloApi2.cpp"
)

setup_tw_library()
