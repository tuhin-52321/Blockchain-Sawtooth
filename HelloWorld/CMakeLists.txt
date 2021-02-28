cmake_minimum_required(VERSION 3.18)
project(HelloWorld LANGUAGES CXX)
include($ENV{TWWorkspaceHome}/TallyWorldPMT/TallyWorld/TWPMT.VSSetup/Wizard/Native/Properties/Common/TWCommon.cmake)
include_directories(${INCLUDE_DIRECTORIES}
    ${TWWorkspaceHome}/TallyWorld/DevSupportPlatform/TWDevSupport.Source/Includes
    ${TWWorkspaceHome}/TallyWorld/UniversalPlatform/TWUniversal.Source/Includes
)
link_directories(${LINK_DIRECTORIES}
    ${TWWorkspaceHome}/TWBuilds/TWLib/TWUniversal
    ${TWWorkspaceHome}/TWBuilds/TWLib/TWDevSupport
)
link_libraries(${LINK_LIBRARIES}
)
add_executable(HelloWorld 
    "HelloWorld.cpp"
)
set_target_properties(HelloWorld PROPERTIES OUTPUT_NAME "${TW_OUTPUT_NAME}")
