//
// Created by Albie on 01/05/2024.
//

#ifndef CS_NATIVE_STRUCT_ALIGN_H
#define CS_NATIVE_STRUCT_ALIGN_H

#ifdef _MSC_VER
#define CSDL_STRUCT __declspec(align(8))
#elif defined(__GNUC__) || defined(__clang__)
#define CSDL_STRUCT __attribute__((aligned(8)))
#else
#error "Unsupported compiler"
#endif

#endif //CS_NATIVE_STRUCT_ALIGN_H
