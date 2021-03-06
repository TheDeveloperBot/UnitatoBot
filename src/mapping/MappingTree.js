const _ = require('lodash');
const Argument = require('mapping/Argument');
const ExecutionContext = require('mapping/ExecutionContext');

module.exports = function (roots) {
	// Public Interface
	this.getRoots = () => roots;

	this.getMessageRoot = (message) => {
		const result = /!(\w+)\s*(.*)$/g.exec(message.content);
		return result === null ? null : result[1];
	};

	this.test = (message) => _.includes(this.getRoots(), this.getMessageRoot(message));

	this.tryExecute = (message) => {
		// Test for valid root
		if (!this.test(message)) {
			return;
		}

		// Try to find a matching mapping
		_.forEach(mappings, (mapping) => {
			const machResult = this.doesMappingMach(message, mapping);

			if (!machResult) {
				return;
			}

			mapping.execute(
				new ExecutionContext(
					this.getMessageRoot(message),
					mapping,
					machResult,
					message
				),
				message.formatting
			);

			// Abort search after first match
			return false;
		});
	};

	this.doesMappingMach = (message, mapping) => {
		const unwrapped = unwrap(message);

		if (unwrapped.length > mapping.args.length) {
			return false;
		}

		const results = [];
		let failed = false;

		_.forEach(mapping.args, (arg, i) => {
			const mach = arg.mach(unwrapped[i]);

			// If any argument fails, this mapping does't matching
			if (mach === false) {
				failed = true;
				return false;
			}

			// If argument maches, store result
			results.push(mach);
		});

		// After a sucessfull mach return results
		return failed ? false : results;
	};

	this.branch = (mapping, executor) => {
		const args = [];

		_.forEach(mapping.split(' '), (arg) => {
			args.push(new Argument(arg));
		});

		mappings.push({
			map: mapping,
			args: args,
			execute: executor
		});

		return this;
	};

	// Internals
	const unwrap = (message) => {
		return /!(\w+)\s*(.*)$/g.exec(message.content)[2].split(' ');
	};

	// Init
	const mappings = [];

	if (!_.isArray(this.getRoots())) {
		throw new Error('Roots argument has to be an array');
	}

	if (_.isEmpty(this.getRoots())) {
		throw new Error('Roots array does not contain any elements');
	}
};
