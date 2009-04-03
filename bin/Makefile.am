TARGETS = \
	libmoon.so \
	System.Windows.dll \
	System.Windows.Controls.dll \
	System.Windows.Controls.Data.dll \
	System.Windows.Browser.dll \
	gtksilver.dll

all: $(TARGETS)

libmoon.so: $(MOONLIGHT_SOURCE)/src/.libs/libmoon.so.0.0.0 ; cp $< $@
gtksilver.dll: $(MOONLIGHT_SOURCE)/gtk/gtksilver.dll ; cp $<{,.mdb} .
%.dll: $(MOONLIGHT_SOURCE)/class/lib/3.0/%.dll ; cp $<{,.mdb} .

clean-local:
	for file in $(TARGETS); do rm -f $$file{,.mdb}; done

run:
	@mono --debug Cubano.exe

MAINTAINERCLEANFILES = Makefile.in
