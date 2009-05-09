#TARGETS = \
#	libmoon.so \
#	System.Windows.dll \
#	System.Windows.Controls.dll \
#	System.Windows.Controls.Data.dll \
#	System.Windows.Browser.dll \
#	Moonlight.Gtk.dll

#bansheedir = $(DESTDIR)$(BANSHEE_PATH)
bansheedir = $(libdir)/banshee-1
banshee_SCRIPTS = \
	Cubano.exe \
	Cubano.exe.mdb

all: $(TARGETS) cubano

#libmoon.so: $(MOONLIGHT_SOURCE)/src/.libs/libmoon.so.0.0.0 ; cp $< $@
#Moonlight.Gtk.dll: $(MOONLIGHT_SOURCE)/gtk/Moonlight.Gtk.dll ; cp $<{,.mdb} .
#%.dll: $(MOONLIGHT_SOURCE)/class/lib/3.0/%.dll ; cp $<{,.mdb} .

clean-local:
	for file in $(TARGETS); do rm -f $$file{,.mdb}; done
	rm -rf addin-db-*
	rm -f Cubano.exe*
	rm -f cubano

cubano: cubano.in
	sed 's,\@BANSHEE_PATH\@,$(BANSHEE_PATH),g' < $< > $@
	chmod +x $@

run: cubano
	./$< --debug --uninstalled

valgrind: cubano
	./$< --valgrind --debug --uninstalled

EXTRA_DIST = \
	cubano.in \
	Mono.Addins.dll \
	Mono.Addins.dll.mdb

MAINTAINERCLEANFILES = Makefile.in

