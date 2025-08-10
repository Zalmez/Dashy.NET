import type {SidebarsConfig} from '@docusaurus/plugin-content-docs';
const sidebars: SidebarsConfig = {
  // Main documentation sidebar
  tutorialSidebar: [
    'intro',
    {
      type: 'category',
      label: '🚀 Getting Started',
      items: [
        {
          type: 'doc',
          id: 'installation',
          label: '⚡ Installation'
        },
        {
          type: 'doc',
          id: 'docker',
          label: '🐳 Docker Deployment'
        },
        {
          type: 'doc',
          id: 'configuration',
          label: '⚙️ Configuration'
        },
        {
          type: 'doc',
          id: 'widgets',
          label: '🧩 Widgets'
        },
      ],
    },
  ],
};

export default sidebars;
