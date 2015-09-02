#!/bin/bash

BUILDTYPE="Debug"
RONN=`which ronn`

if [ -z "$RONN" ]; then
    >&2 echo "Please install the ruby-ronn package (sudo apt-get ruby-ronn) to create the manpage."
    exit 2
fi

PACKAGENAME=`grep Package debian/control | cut -d' ' -f2-`
PACKAGEVERSION=`grep Version debian/control | cut -d' ' -f2-`
PACKAGEARCH=`grep Architecture debian/control | cut -d' ' -f2-`
DEBFILENAME="${PACKAGENAME}_${PACKAGEVERSION}_${PACKAGEARCH}.deb"

umask 022

SRCDIR="Cog.Application.CommandLine"
BUILDDIR="$SRCDIR/bin/$BUILDTYPE"
WORKDIR=`mktemp -d`
BASEDIR="$WORKDIR/usr/lib/$PACKAGENAME"
BINDIR="$WORKDIR/usr/bin"
DEBDIR="$WORKDIR/DEBIAN"
DOCDIR="$WORKDIR/usr/share/doc/$PACKAGENAME"
MANDIR="$WORKDIR/usr/share/man/man1"
mkdir -p "$BASEDIR" "$BINDIR" "$DEBDIR" "$DOCDIR" "$MANDIR"

cp "$SRCDIR/bin/$BUILDTYPE"/* "$BASEDIR"
chmod -x "$BASEDIR"/*.dll
cp debian/control "$DEBDIR"
cp debian/copyright "$DOCDIR"
cp debian/changelog "$DOCDIR/changelog.Debian"
gzip -9 "$DOCDIR/changelog.Debian"
ronn < "$SRCDIR/README.md" > "$MANDIR/${PACKAGENAME}.1"
gzip -9 "$MANDIR/${PACKAGENAME}.1"

cat > "$BINDIR/$PACKAGENAME" <<EOF
#!/bin/sh
exec /usr/bin/cli /usr/lib/$PACKAGENAME/${PACKAGENAME}.exe "\$@"
EOF
chmod +x "$BINDIR/$PACKAGENAME"

fakeroot dpkg-deb -b "$WORKDIR"

mv "${WORKDIR}.deb" "$DEBFILENAME"
rm -rf "$WORKDIR" 2>/dev/null

lintian "$DEBFILENAME"
