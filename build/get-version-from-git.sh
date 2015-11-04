#!/bin/bash
vn_full=$(git describe --match="v[0-9]*" --long $BUILD_VCS_NUMBER)
git_sha=`echo -n "$vn_full" | rev | cut -d- -f1 | rev | cut -c2-`
commit_count=`echo -n "$vn_full" | rev | cut -d- -f2 | rev `
released_version=`echo -n "$vn_full" | rev | cut -d- -f3- | rev | cut -c2-`
echo "##teamcity[setParameter name='env.GIT_VN_FULL' value='$vn_full']"
echo "##teamcity[setParameter name='env.GIT_VN_TAG' value='$released_version']"
echo "##teamcity[setParameter name='env.GIT_VN_COMMITS' value='$commit_count']"
echo "##teamcity[setParameter name='env.GIT_VN_SHA' value='$git_sha']"
