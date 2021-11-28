source "https://rubygems.org"

gem "jekyll", "~> 3.9"

gem "github-pages", "~> 221", group: :jekyll_plugins

group :jekyll_plugins do
  gem "jekyll-feed", "~> 0.15"
end

install_if -> { RUBY_PLATFORM =~ %r!mingw|mswin|java! } do
  gem "tzinfo", "~> 2.0"
  gem "tzinfo-data"
end

gem "wdm", "~> 0.1.0", :install_if => Gem.win_platform?

gem "kramdown-parser-gfm"
