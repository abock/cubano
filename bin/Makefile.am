bansheedir = $(libdir)/banshee-1
banshee_SCRIPTS = \
	Cubano.exe \
	Cubano.exe.mdb

all: $(TARGETS) clutter-sharp cubano

clutter-sharp:
	@for lib in $(CLUTTER_LIBS); do \
		cp $$(dirname $$(readlink -e $${lib:3}))/* .; \
	done

clean-local:
	for file in $(TARGETS); do rm -f $$file{,.mdb}; done
	rm -rf addin-db-*
	rm -f Cubano.exe*
	rm -f cubano
	rm -rf clutter-*sharp*
	rm -rf glib-sharp*

cubano: cubano.in
	sed 's,\@BANSHEE_PATH\@,$(BANSHEE_PATH),g' < $< > $@
	chmod +x $@

run: cubano
	./$< --debug --uninstalled

valgrind: cubano
	./$< --valgrind --debug --uninstalled

gdb: cubano
	./$< --gdb --debug --uninstalled

EXTRA_DIST = \
	cubano.in \
	Mono.Addins.dll \
	Mono.Addins.dll.mdb

MAINTAINERCLEANFILES = Makefile.in

