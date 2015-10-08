#!/usr/bin/env python

from __future__ import print_function

# Edit these constants if desired. NOTE that if you change DEFAULT_TAG_FORMAT,
# you'll need to change the .lstrip('v") part of parse_tag() as well.
DEFAULT_TAG_FORMAT="v[0-9]*"  # Shell glob format, not regex
DEFAULT_VERSION_IF_NO_TAGS="0.0.0"

import subprocess
import os

def cmd_output(cmd):
    p = subprocess.Popen(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    (stdout, stderr) = p.communicate()
    return stdout.rstrip()  # Trim newlines from end of output

def git_describe(commitish = None):
    "Run `git describe` (on commitish, if passed) and return its stdout"
    cmd = ['git', 'describe', '--long', '--match={}'.format(DEFAULT_TAG_FORMAT), '--always']
    if commitish:
        cmd.append(commitish)
    return cmd_output(cmd)

def git_commit_count():
    "Get total commit count in repo"
    cmd = ['git', 'rev-list', 'HEAD', '--count']
    return cmd_output(cmd)

def parse_tag(git_tag):
    "Parse git describe output into its component parts"
    result = { 'GIT_VN_FULL': git_tag }
    parts = git_tag.split('-')
    if len(parts) == 1:
        # No version tags found; build our own
        result['GIT_VN_SHA'] = parts[0]
        result['GIT_VN_COMMITS'] = git_commit_count()
        result['GIT_VN_TAG'] = DEFAULT_VERSION_IF_NO_TAGS
        # Reconstruct GIT_VN_FULL to match normal "git describe" output
        result['GIT_VN_FULL'] = "v{GIT_VN_TAG}-{GIT_VN_COMMITS}-g{GIT_VN_SHA}".format(**result)
    else:
        result['GIT_VN_SHA'] = parts[-1].lstrip('g')
        result['GIT_VN_COMMITS'] = parts[-2]
        result['GIT_VN_TAG'] = '-'.join(parts[:-2]).lstrip('v')
    return result

def teamcity_log(tag_parts):
    for name, val in tag_parts.iteritems():
        print("##teamcity[setParameter name='env.{}' value='{}']".format(name, val))

def console_log(tag_parts):
    for name, val in tag_parts.iteritems():
        print("{}={}".format(name, val))

def is_running_under_teamcity():
    "Return True if we're running in a TeamCity agent environment"
    return os.environ.has_key('TEAMCITY_VERSION')

if __name__ == '__main__':
    output = git_describe()
    parts = parse_tag(output)
    if is_running_under_teamcity():
        teamcity_log(parts)
    else:
        console_log(parts)
