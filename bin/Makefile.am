TARGETS = \
	libmoon.so \
	System.Windows.dll \
	System.Windows.Controls.dll \
	System.Windows.Controls.Data.dll \
	System.Windows.Browser.dll \
	Moonlight.Gtk.dll

all: $(TARGETS)

libmoon.so: $(MOONLIGHT_SOURCE)/src/.libs/libmoon.so.0.0.0 ; cp $< $@
Moonlight.Gtk.dll: $(MOONLIGHT_SOURCE)/gtk/Moonlight.Gtk.dll ; cp $<{,.mdb} .
%.dll: $(MOONLIGHT_SOURCE)/class/lib/3.0/%.dll ; cp $<{,.mdb} .

clean-local:
	for file in $(TARGETS); do rm -f $$file{,.mdb}; done

run:
	@LD_LIBRARY_PATH=$$PWD:$(BANSHEE_PATH) MONO_PATH=$(BANSHEE_PATH):$(BANSHEE_PATH)/Extension:$(BANSHEE_PATH)/Backends mono --debug Cubano.exe

MAINTAINERCLEANFILES = Makefile.in
