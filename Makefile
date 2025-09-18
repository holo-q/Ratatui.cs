SHELL := /bin/bash

# Allow overriding build profile: `make PROFILE=debug`
PROFILE ?= release

CARGO_MANIFEST := native/ratatui-ffi/Cargo.toml
TARGET_DIR     := native/ratatui-ffi/target
BUILD_DIR      := $(TARGET_DIR)/$(PROFILE)

UNAME_S := $(shell uname -s)
UNAME_M := $(shell uname -m)

ifeq ($(UNAME_S),Linux)
  ifeq ($(UNAME_M),x86_64)
    RID := linux-x64
  else ifeq ($(UNAME_M),aarch64)
    RID := linux-arm64
  else
    $(error Unsupported Linux architecture: $(UNAME_M))
  endif
  LIB_NAME := libratatui_ffi.so
else ifeq ($(UNAME_S),Darwin)
  ifeq ($(UNAME_M),x86_64)
    RID := osx-x64
  else ifeq ($(UNAME_M),arm64)
    RID := osx-arm64
  else
    $(error Unsupported macOS architecture: $(UNAME_M))
  endif
  LIB_NAME := libratatui_ffi.dylib
else ifneq (,$(findstring MINGW,$(UNAME_S)))
  ifneq ($(UNAME_M),x86_64)
    $(warning Treating $(UNAME_M) as x64 for Windows build)
  endif
  RID := win-x64
  LIB_NAME := ratatui_ffi.dll
else
  $(error Unsupported platform: $(UNAME_S))
endif

OUTPUT_LIB := $(BUILD_DIR)/$(LIB_NAME)
RUNTIME_DIR := src/Ratatui/runtimes/$(RID)/native
INSTALLED_LIB := $(RUNTIME_DIR)/$(LIB_NAME)

CARGO_FLAGS := --manifest-path $(CARGO_MANIFEST)
ifeq ($(PROFILE),release)
  CARGO_FLAGS += --release
endif

.PHONY: all native copy clean print-rid

all: copy

native:
	@echo "Building ratatui_ffi ($(PROFILE)) for $(RID)…"
	cargo build $(CARGO_FLAGS)

copy: native
	@if [ ! -f "$(OUTPUT_LIB)" ]; then \
		echo "Expected native artifact $(OUTPUT_LIB) was not produced." >&2; \
		exit 1; \
	fi
	@echo "Copying $(OUTPUT_LIB) => $(INSTALLED_LIB)"
	@mkdir -p "$(RUNTIME_DIR)"
	@cp "$(OUTPUT_LIB)" "$(INSTALLED_LIB)"

clean:
	cargo clean --manifest-path $(CARGO_MANIFEST)
	@rm -f "$(INSTALLED_LIB)"

print-rid:
	@echo $(RID)
