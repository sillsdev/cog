#!/bin/bash

BUILDTYPE="Debug"
RONN=`which ronn`

if [ -z "$RONN" ]; then
    >&2 echo "Please install the ruby-ronn package (sudo apt-get ruby-ronn) to create the manpage."
    exit 2
fi

get_upstream_version() {
    echo "$1" | rev | cut -d- -f2- | rev
}

get_debian_revision() {
    echo "$1" | rev | cut -d- -f1 | rev
}

get_version_from_changelog() {
    dpkg-parsechangelog | grep "Version: " | cut -d' ' -f2
}

CHANGELOG_MSG=${1:-"Changelog automatically updated by TeamCity (build number ${BUILD_NUMBER})"}

PACKAGENAME=`grep Package debian/control | cut -d' ' -f2-`
PACKAGEVERSION=`grep Version debian/control | cut -d' ' -f2-`
UPSTREAMVERSION=`get_upstream_version $PACKAGEVERSION`
OLD_PACKAGEVERSION=`get_version_from_changelog`
OLD_UPSTREAMVERSION=`get_upstream_version $OLD_PACKAGEVERSION`
PACKAGEARCH=`grep Architecture debian/control | cut -d' ' -f2-`

# If upstream package version hasn't incremented, up Debian revision; else start Debian revision over from 1
if [ "$UPSTREAMVERSION" = "$OLD_UPSTREAMVERSION" ]; then
    dch -U --maintmaint -i "$CHANGELOG_MSG"
else
    dch -U --maintmaint -v "${UPSTREAMVERSION}-1" "$CHANGELOG_MSG"
fi

# Finalize the changelog so we get a new Debian revision number, and thus a new .deb, next time
dch -U -r --vendor Debian --maintmaint "$CHANGELOG_MSG"

NEW_PACKAGEVERSION=`get_version_from_changelog`
DEBIAN_REVISION=`get_debian_revision "$NEW_PACKAGEVERSION"`
DEBFILENAME="${PACKAGENAME}_${NEW_PACKAGEVERSION}_${PACKAGEARCH}.deb"

echo "About to create package $DEBFILENAME"

umask 022

# Set up necessary directories inside the package
SRCDIR="Cog.Application.CommandLine"
BUILDDIR="$SRCDIR/bin/$BUILDTYPE"
WORKDIR=`mktemp -d`
BASEDIR="$WORKDIR/usr/lib/$PACKAGENAME"
BINDIR="$WORKDIR/usr/bin"
DEBDIR="$WORKDIR/DEBIAN"
DOCDIR="$WORKDIR/usr/share/doc/$PACKAGENAME"
MANDIR="$WORKDIR/usr/share/man/man1"
OUTDIR="output"
mkdir -p "$BASEDIR" "$BINDIR" "$DEBDIR" "$DOCDIR" "$MANDIR" "$OUTDIR"

# Put the package contents together
cp "$SRCDIR/bin/$BUILDTYPE"/* "$BASEDIR"
chmod -x "$BASEDIR"/*.dll
sed -e "s/DEBIAN_REVISION/${DEBIAN_REVISION}/" debian/control > "$DEBDIR/control"
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

# Clean up old packages, if any, in output directory, then put new package together
rm $OUTDIR/${PACKAGENAME}_*_${PACKAGEARCH}.deb
fakeroot dpkg-deb -b "$WORKDIR"
mv "${WORKDIR}.deb" "$OUTDIR/$DEBFILENAME"
rm -rf "$WORKDIR" 2>/dev/null

lintian "$OUTDIR/$DEBFILENAME"
