cmake_minimum_required(VERSION 3.19)
project(CoreProcessSample LANGUAGES CXX)
set(CMAKE_MESSAGE_LOG_LEVEL NOTICE)

include($ENV{TWWorkspaceHome}/TallyWorldPMT/TallyWorld/TWPMT.VSSetup/Wizard/Native/Properties/Common/TWCommon.cmake)

set(PROJECT_INCLUDES
)

set(DEPENDENT_LAYERS
   TWCoreProcess,Source,Static,TallyWorld/FoundationalPlatform/TWCoreProcess.Source,Includes
   TWCoreIO,Source,Static,TallyWorld/FoundationalPlatform/TWCoreIO.Source,Includes
   TWCoreData,Source,Static,TallyWorld/FoundationalPlatform/TWCoreData.Source,Includes
   TWCore,Source,Static,TallyWorld/FoundationalPlatform/TWCore.Source,Includes
   TWDevSupport,Source,Static,TallyWorld/DevSupportPlatform/TWDevSupport.Source,Includes
   TWUniversal,Source,Static,TallyWorld/UniversalPlatform/TWUniversal.Source,TWUniversal/Includes
)

add_executable(CoreProcessSample
    "Source.cpp"
)

setup_tw_executable()
