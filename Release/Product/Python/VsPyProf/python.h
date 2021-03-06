/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

#ifndef __PYTHON_H__
#define __PYTHON_H__

// defines limited header of Python API for compatible access across a number of Pythons.

class PyTypeObject;
class PyThreadState;

#define PyObject_HEAD				\
	size_t ob_refcnt;				\
	PyTypeObject *ob_type;

#define PyObject_VAR_HEAD		\
	PyObject_HEAD			\
	size_t ob_size; /* Number of items in variable part */

class PyObject {
public:
	PyObject_HEAD
};

class PyVarObject : public PyObject {
public:
	size_t ob_size; /* Number of items in variable part */
};

// 2.4 - 2.7 compatible
class PyCodeObject24_27 : public PyObject {
public:
    int co_argcount;		/* #arguments, except *args */
    int co_nlocals;		/* #local variables */
    int co_stacksize;		/* #entries needed for evaluation stack */
    int co_flags;		/* CO_..., see below */
    PyObject *co_code;		/* instruction opcodes */
    PyObject *co_consts;	/* list (constants used) */
    PyObject *co_names;		/* list of strings (names used) */
    PyObject *co_varnames;	/* tuple of strings (local variable names) */
    PyObject *co_freevars;	/* tuple of strings (free variable names) */
    PyObject *co_cellvars;      /* tuple of strings (cell variable names) */
    /* The rest doesn't count for hash/cmp */
    PyObject *co_filename;	/* string (where it was loaded from) */
    PyObject *co_name;		/* string (name, for reference) */
    int co_firstlineno;		/* first source line number */
    PyObject *co_lnotab;	/* string (encoding addr<->lineno mapping) */
};

// 3.1+
class PyCodeObject3k : public PyObject {
public:
    int co_argcount;		/* #arguments, except *args */
    int co_kwonlyargcount;	/* #keyword only arguments */
    int co_nlocals;		/* #local variables */
    int co_stacksize;		/* #entries needed for evaluation stack */
    int co_flags;		/* CO_..., see below */
    PyObject *co_code;		/* instruction opcodes */
    PyObject *co_consts;	/* list (constants used) */
    PyObject *co_names;		/* list of strings (names used) */
    PyObject *co_varnames;	/* tuple of strings (local variable names) */
    PyObject *co_freevars;	/* tuple of strings (free variable names) */
    PyObject *co_cellvars;      /* tuple of strings (cell variable names) */
    /* The rest doesn't count for hash or comparisons */
    PyObject *co_filename;	/* unicode (where it was loaded from) */
    PyObject *co_name;		/* unicode (name, for reference) */
    int co_firstlineno;		/* first source line number */
    PyObject *co_lnotab;	/* string (encoding addr<->lineno mapping) */
    void *co_zombieframe;     /* for optimization only (see frameobject.c) */
};

// 2.4 - 3.1
class PyFunctionObject : public PyObject {
public:
    PyObject *func_code;	/* A code object */
};

// 2.4 - 2.7 compatible
class PyStringObject : public PyVarObject {
public:
    long ob_shash;
    int ob_sstate;
    char ob_sval[1];

    /* Invariants:
     *     ob_sval contains space for 'ob_size+1' elements.
     *     ob_sval[ob_size] == 0.
     *     ob_shash is the hash of the string or -1 if not computed yet.
     *     ob_sstate != 0 iff the string object is in stringobject.c's
     *       'interned' dictionary; in this case the two references
     *       from 'interned' to this object are *not counted* in ob_refcnt.
     */
};

// 2.4 - 3.1 compatible
typedef struct {
    PyObject_HEAD
    size_t length;		/* Length of raw Unicode data in buffer */
    wchar_t *str;		/* Raw Unicode buffer */
    long hash;			/* Hash value; -1 if not set */
} PyUnicodeObject;

// 2.4 - 3.1 compatible
class PyFrameObject : public PyVarObject {
public:
    PyFrameObject *f_back;	/* previous frame, or NULL */
	union {
		PyCodeObject24_27 *f_code_24_27;	/* code segment */
		PyCodeObject3k    *f_code_3k;		/* code segment */
	};
    PyObject *f_builtins;	/* builtin symbol table (PyDictObject) */
    PyObject *f_globals;	/* global symbol table (PyDictObject) */
    PyObject *f_locals;		/* local symbol table (any mapping) */
	PyObject **f_valuestack;	/* points after the last local */
    /* Next free slot in f_valuestack.  Frame creation sets to f_valuestack.
       Frame evaluation usually NULLs it, but a frame that yields sets it
       to the current stack top. */
    PyObject **f_stacktop;
    PyObject *f_trace;		/* Trace function */
    PyObject *f_exc_type, *f_exc_value, *f_exc_traceback;
    PyThreadState* f_tstate;
    int f_lasti;		/* Last instruction if called */
    /* As of 2.3 f_lineno is only valid when tracing is active (i.e. when
       f_trace is set) -- at other times use PyCode_Addr2Line instead. */
    int f_lineno;		/* Current line number */
};

#define CO_MAXBLOCKS 20
typedef struct {
    int b_type;			/* what kind of block this is */
    int b_handler;		/* where to jump to find handler */
    int b_level;		/* value stack level to pop to */
} PyTryBlock;

class PyFrameObject24 : public PyFrameObject {
public:
    int f_restricted;		/* Flag set if restricted operations
				   in this scope */
    int f_iblock;		/* index in f_blockstack */
    PyTryBlock f_blockstack[CO_MAXBLOCKS]; /* for try and loop blocks */
    int f_nlocals;		/* number of locals */
    int f_ncells;
    int f_nfreevars;
    int f_stacksize;		/* size of value stack */
    PyObject *f_localsplus[1];	/* locals+stack, dynamically sized */
};

class PyFrameObject25_31 : public PyFrameObject {
public:
    int f_iblock;		/* index in f_blockstack */
    PyTryBlock f_blockstack[CO_MAXBLOCKS]; /* for try and loop blocks */
    PyObject *f_localsplus[1];	/* locals+stack, dynamically sized */
};

typedef void (*destructor)(PyObject *);

// 2.4 - 3.1
class PyMethodDef {
public:
    char	*ml_name;	/* The name of the built-in function/method */
};


// 
// 2.4 - 3.1, 2.4 has different compat in 64-bit but we don't support any of the released 64-bit platforms (which includes only IA-64)
// While these are compatible there are fields only available on later versions.
class PyTypeObject : public PyVarObject {
public:
	const char *tp_name; /* For printing, in format "<module>.<name>" */
	size_t tp_basicsize, tp_itemsize; /* For allocation */

	/* Methods to implement standard operations */

	destructor tp_dealloc;
	void* tp_print;
	void* tp_getattr;
	void* tp_setattr;
	void* tp_compare;
	void* tp_repr;

	/* Method suites for standard classes */

	void *tp_as_number;
	void*tp_as_sequence;
	void*tp_as_mapping;

	/* More standard operations (here for binary compatibility) */

	void* tp_hash;
	void* tp_call;
	void* tp_str;
	void* tp_getattro;
	void* tp_setattro;

	/* Functions to access object as input/output buffer */
	void*tp_as_buffer;

	/* Flags to define presence of optional/expanded features */
	long tp_flags;

	const char *tp_doc; /* Documentation string */

	/* Assigned meaning in release 2.0 */
	/* call function for all accessible objects */
	void*  tp_traverse;

	/* delete references to contained objects */
	void* tp_clear;

	/* Assigned meaning in release 2.1 */
	/* rich comparisons */
	void*  tp_richcompare;

	/* weak reference enabler */
	size_t tp_weaklistoffset;

	/* Added in release 2.2 */
	/* Iterators */
	void*  tp_iter;
	void*  tp_iternext;

	/* Attribute descriptor and subclassing stuff */
	PyMethodDef *tp_methods;
	struct PyMemberDef *tp_members;
	struct PyGetSetDef *tp_getset;
	struct _typeobject *tp_base;
	PyObject *tp_dict;
	void*  tp_descr_get;
	void*  tp_descr_set;
	size_t tp_dictoffset;
	void* tp_init;
	void* tp_alloc;
	void* tp_new;
	void* tp_free; /* Low-level free-memory routine */
	void* tp_is_gc; /* For PyObject_IS_GC */
	PyObject *tp_bases;
	PyObject *tp_mro; /* method resolution order */
	PyObject *tp_cache;
	PyObject *tp_subclasses;
	PyObject *tp_weaklist;
	void*  tp_del;

	/* Type attribute cache version tag. Added in version 2.6 */
	unsigned int tp_version_tag;
};

// 2.4 - 3-1
class PyTupleObject : public PyVarObject {
public:
    PyObject *ob_item[1];

    /* ob_item contains space for 'ob_size' elements.
     * Items must normally not be NULL, except during construction when
     * the tuple is not yet visible outside the function that builds it.
     */
};

// 2.4 - 3.1
class PyCFunctionObject : public PyObject {
public:
    PyMethodDef *m_ml;		/* Description of the C function to call */
    PyObject    *m_self;	/* Passed as 'self' arg to the C func, can be NULL */
    PyObject    *m_module;	/* The __module__ attribute, can be anything */
};

typedef int (*Py_tracefunc)(PyObject *, PyFrameObject *, int, PyObject *);

#define PyTrace_CALL 0
#define PyTrace_EXCEPTION 1
#define PyTrace_LINE 2
#define PyTrace_RETURN 3
#define PyTrace_C_CALL 4
#define PyTrace_C_EXCEPTION 5
#define PyTrace_C_RETURN 6

class PyInterpreterState {
};

class PyThreadState_24 {
public:
    /* See Python/ceval.c for comments explaining most fields */

    struct _ts *next;
    PyInterpreterState *interp;

    PyFrameObject *frame;
    int recursion_depth;
    /* 'tracing' keeps track of the execution depth when tracing/profiling.
       This is to prevent the actual trace/profile code from being recorded in
       the trace/profile. */
    int tracing;
    int use_tracing;

    Py_tracefunc c_profilefunc;
    Py_tracefunc c_tracefunc;
    PyObject *c_profileobj;
    PyObject *c_traceobj;

    PyObject *curexc_type;
    PyObject *curexc_value;
    PyObject *curexc_traceback;

    PyObject *exc_type;
    PyObject *exc_value;
    PyObject *exc_traceback;

    PyObject *dict;  /* Stores per-thread state */

    /* tick_counter is incremented whenever the check_interval ticker
     * reaches zero. The purpose is to give a useful measure of the number
     * of interpreted bytecode instructions in a given thread.  This
     * extremely lightweight statistic collector may be of interest to
     * profilers (like psyco.jit()), although nothing in the core uses it.
     */
    int tick_counter;

    int gilstate_counter;

    PyObject *async_exc; /* Asynchronous exception to raise */
    long thread_id; /* Thread id where this tstate was created */

    /* XXX signal handlers should also be here */
};

class PyThreadState_25_27 {
public:
    /* See Python/ceval.c for comments explaining most fields */

    struct _ts *next;
    PyInterpreterState *interp;

    PyFrameObject *frame;
    int recursion_depth;
    /* 'tracing' keeps track of the execution depth when tracing/profiling.
       This is to prevent the actual trace/profile code from being recorded in
       the trace/profile. */
    int tracing;
    int use_tracing;

    Py_tracefunc c_profilefunc;
    Py_tracefunc c_tracefunc;
    PyObject *c_profileobj;
    PyObject *c_traceobj;

    PyObject *curexc_type;
    PyObject *curexc_value;
    PyObject *curexc_traceback;

    PyObject *exc_type;
    PyObject *exc_value;
    PyObject *exc_traceback;

    PyObject *dict;  /* Stores per-thread state */

    /* tick_counter is incremented whenever the check_interval ticker
     * reaches zero. The purpose is to give a useful measure of the number
     * of interpreted bytecode instructions in a given thread.  This
     * extremely lightweight statistic collector may be of interest to
     * profilers (like psyco.jit()), although nothing in the core uses it.
     */
    int tick_counter;

    int gilstate_counter;

    PyObject *async_exc; /* Asynchronous exception to raise */
    long thread_id; /* Thread id where this tstate was created */

    /* XXX signal handlers should also be here */
};

class PyThreadState_30_31 {
public:
    struct _ts *next;
    PyInterpreterState *interp;

    PyFrameObject *frame;
    int recursion_depth;
    char overflowed; /* The stack has overflowed. Allow 50 more calls
		        to handle the runtime error. */
    char recursion_critical; /* The current calls must not cause 
				a stack overflow. */
    /* 'tracing' keeps track of the execution depth when tracing/profiling.
       This is to prevent the actual trace/profile code from being recorded in
       the trace/profile. */
    int tracing;
    int use_tracing;

    Py_tracefunc c_profilefunc;
    Py_tracefunc c_tracefunc;
    PyObject *c_profileobj;
    PyObject *c_traceobj;

    PyObject *curexc_type;
    PyObject *curexc_value;
    PyObject *curexc_traceback;

    PyObject *exc_type;
    PyObject *exc_value;
    PyObject *exc_traceback;

    PyObject *dict;  /* Stores per-thread state */

    /* tick_counter is incremented whenever the check_interval ticker
     * reaches zero. The purpose is to give a useful measure of the number
     * of interpreted bytecode instructions in a given thread.  This
     * extremely lightweight statistic collector may be of interest to
     * profilers (like psyco.jit()), although nothing in the core uses it.
     */
    int tick_counter;

    int gilstate_counter;

    PyObject *async_exc; /* Asynchronous exception to raise */
    long thread_id; /* Thread id where this tstate was created */

    /* XXX signal handlers should also be here */
};

class PyThreadState {
public:
    union {
        PyThreadState_24 _24;
        PyThreadState_25_27 _25_27;
        PyThreadState_30_31 _30_31;
    };
};

class PyIntObject : public PyObject {
public:
    long ob_ival;
};

class Py3kLongObject : public PyVarObject {
public:
	DWORD ob_digit[1];
};

#endif
