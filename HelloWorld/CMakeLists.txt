cmake_minimum_required(VERSION 3.18)
project(HelloWorld LANGUAGES CXX)
include($ENV{TWWorkspaceHome}/TallyWorldPMT/TallyWorld/TWPMT.VSSetup/Wizard/Native/Properties/Common/TWCommon.cmake)

set(PROJECT_INCLUDES
    .
)

set(DEPENDENT_LAYERS
   TWDevSupport,Source,Static,TallyWorld/DevSupportPlatform/TWDevSupport.Source/Includes
   TWUniversalPremise,Source,Static,TallyWorld/UniversalPlatform/TWUniversal.Source,TWUniversal/Includes
)

add_library(HelloWorld STATIC
    "HelloWorld.cpp"
)

setup_tw_library()
