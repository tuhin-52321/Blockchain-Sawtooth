cmake_minimum_required(VERSION 3.20)
project(Project1 LANGUAGES CXX)

set(VERBOSE 1)
set(CMAKE_VERBOSE_MAKEFILE ON)

message(NOTICE ${CMAKE_CXX_FLAGS})
message(NOTICE "Debug ->   ${CMAKE_CXX_FLAGS_DEBUG}")
message(NOTICE "Release -> ${CMAKE_CXX_FLAGS_RELEASE}")
message(NOTICE ${CMAKE_CXX_FLAGS_TWDEBUG})
message(NOTICE ${CMAKE_CXX_FLAGS_TWSIMULATION})
message(NOTICE ${CMAKE_CXX_FLAGS_TWMETRICSONLY})
message(NOTICE ${CMAKE_CXX_FLAGS_TWRELEASE})


set(CMAKE_VERBOSE_MAKEFILE ON)

add_executable(Project1
    "main.cpp"
)

target_include_directories(Project1
    PRIVATE Includes
)

